package application

import (
	"context"
	"fmt"
	"image"
	_ "image/gif"
	_ "image/jpeg"
	_ "image/png"
	"io"
	"time"

	"github.com/google/uuid"
	_ "golang.org/x/image/webp"

	"media-service/internal/domain"
)

const presignedUploadExpiry = 15 * time.Minute

type MediaService struct {
	repo    MediaRepository
	storage ObjectStorage
	events  EventPublisher
	bucket  string
}

func NewMediaService(repo MediaRepository, storage ObjectStorage, events EventPublisher, bucket string) *MediaService {
	return &MediaService{repo: repo, storage: storage, events: events, bucket: bucket}
}

func (s *MediaService) RequestUpload(ctx context.Context, in RequestUploadInput) (*RequestUploadOutput, error) {
	if in.ContentType == "" || in.UploadedBy == "" {
		return nil, fmt.Errorf("%w: content_type and uploaded_by are required", domain.ErrInvalidInput)
	}
	if _, err := uuid.Parse(in.UploadedBy); err != nil {
		return nil, fmt.Errorf("%w: uploaded_by must be uuid", domain.ErrInvalidInput)
	}
	if in.MediaType != domain.MediaTypeImage && in.MediaType != domain.MediaTypeVideo {
		return nil, fmt.Errorf("%w: invalid media_type", domain.ErrInvalidInput)
	}

	id := uuid.NewString()
	objectKey := fmt.Sprintf("%s/%s", mediaTypeFolder(in.MediaType), id)

	asset := &domain.MediaAsset{
		ID:          id,
		Bucket:      s.bucket,
		ObjectKey:   objectKey,
		MediaType:   in.MediaType,
		ContentType: in.ContentType,
		Status:      domain.StatusPendingUpload,
		Checksum:    in.Checksum,
		UploadedBy:  in.UploadedBy,
		CreatedAt:   time.Now().UTC(),
	}

	if err := s.repo.CreateAsset(ctx, asset); err != nil {
		return nil, fmt.Errorf("create asset: %w", err)
	}

	url, err := s.storage.GeneratePresignedUploadURL(ctx, s.bucket, objectKey, in.ContentType, presignedUploadExpiry)
	if err != nil {
		return nil, fmt.Errorf("generate presigned url: %w", err)
	}

	return &RequestUploadOutput{
		AssetID:       id,
		UploadURL:     url,
		Bucket:        s.bucket,
		ObjectKey:     objectKey,
		ExpiresInSecs: int(presignedUploadExpiry.Seconds()),
	}, nil
}

func (s *MediaService) ConfirmUpload(ctx context.Context, in ConfirmUploadInput) (*domain.MediaAsset, error) {
	asset, err := s.repo.GetAssetByID(ctx, in.AssetID)
	if err != nil {
		return nil, err
	}
	if asset.Status != domain.StatusPendingUpload && asset.Status != domain.StatusFailed {
		return nil, fmt.Errorf("%w: asset is not awaiting upload (status=%s)", domain.ErrConflict, asset.Status)
	}

	info, err := s.storage.HeadObject(ctx, asset.Bucket, asset.ObjectKey)
	if err != nil {
		asset.Status = domain.StatusFailed
		_ = s.repo.UpdateAsset(ctx, asset)
		_ = s.events.PublishAssetFailed(ctx, asset)
		return nil, fmt.Errorf("%w: object not found in storage", domain.ErrUploadNotReady)
	}

	size := info.SizeBytes
	asset.SizeBytes = &size
	if info.ContentType != "" {
		asset.ContentType = info.ContentType
	}
	if asset.MediaType == domain.MediaTypeImage {
		if w, h, err := s.detectImageDimensions(ctx, asset.Bucket, asset.ObjectKey); err == nil {
			asset.Width = &w
			asset.Height = &h
		}
	}
	if in.Width != nil {
		asset.Width = in.Width
	}
	if in.Height != nil {
		asset.Height = in.Height
	}
	if in.DurationSeconds != nil {
		asset.DurationSeconds = in.DurationSeconds
	}
	asset.Status = domain.StatusReady

	if err := s.repo.UpdateAsset(ctx, asset); err != nil {
		return nil, fmt.Errorf("update asset: %w", err)
	}

	_ = s.events.PublishAssetReady(ctx, asset)
	return asset, nil
}

func (s *MediaService) GetAsset(ctx context.Context, id string) (asset *domain.MediaAsset, publicURL string, err error) {
	asset, err = s.repo.GetAssetByID(ctx, id)
	if err != nil {
		return nil, "", err
	}
	if asset.Status == domain.StatusReady {
		publicURL = s.storage.GetPublicURL(asset.Bucket, asset.ObjectKey)
	}
	return asset, publicURL, nil
}

func (s *MediaService) CreateAttachment(ctx context.Context, in CreateAttachmentInput) (*domain.MediaAttachment, error) {
	if in.MediaAssetID == "" || in.OwnerService == "" || in.OwnerType == "" || in.OwnerID == "" || in.Role == "" {
		return nil, fmt.Errorf("%w: media_asset_id, owner_service, owner_type, owner_id, role are required", domain.ErrInvalidInput)
	}

	asset, err := s.repo.GetAssetByID(ctx, in.MediaAssetID)
	if err != nil {
		return nil, err
	}
	if asset.Status != domain.StatusReady {
		return nil, fmt.Errorf("%w: media asset is not READY yet", domain.ErrConflict)
	}

	attachment := &domain.MediaAttachment{
		ID:           uuid.NewString(),
		MediaAssetID: in.MediaAssetID,
		OwnerService: in.OwnerService,
		OwnerType:    in.OwnerType,
		OwnerID:      in.OwnerID,
		Role:         in.Role,
		Position:     in.Position,
		CreatedAt:    time.Now().UTC(),
	}

	if err := s.repo.CreateAttachment(ctx, attachment); err != nil {
		return nil, fmt.Errorf("create attachment: %w", err)
	}
	_ = s.events.PublishAttachmentCreated(ctx, attachment)
	return attachment, nil
}

func (s *MediaService) ListAttachments(ctx context.Context, filter AttachmentFilter) ([]*domain.MediaAttachment, error) {
	if filter.OwnerService == "" || filter.OwnerType == "" || filter.OwnerID == "" {
		return nil, fmt.Errorf("%w: owner_service, owner_type, owner_id are required", domain.ErrInvalidInput)
	}
	return s.repo.ListAttachments(ctx, filter)
}

func (s *MediaService) DeleteAttachment(ctx context.Context, id string) error {
	if _, err := s.repo.GetAttachmentByID(ctx, id); err != nil {
		return err
	}
	if err := s.repo.DeleteAttachment(ctx, id); err != nil {
		return err
	}
	_ = s.events.PublishAttachmentDeleted(ctx, id) // best-effort
	return nil
}

func mediaTypeFolder(mt domain.MediaType) string {
	if mt == domain.MediaTypeVideo {
		return "videos"
	}
	return "images"
}

func (s *MediaService) detectImageDimensions(ctx context.Context, bucket, objectKey string) (width, height int, err error) {
	reader, err := s.storage.GetObjectStream(ctx, bucket, objectKey)
	if err != nil {
		return 0, 0, err
	}
	defer func(reader io.ReadCloser) {
		err := reader.Close()
		if err != nil {
		}
	}(reader)

	cfg, _, err := image.DecodeConfig(reader)
	if err != nil {
		return 0, 0, err
	}
	return cfg.Width, cfg.Height, nil
}
