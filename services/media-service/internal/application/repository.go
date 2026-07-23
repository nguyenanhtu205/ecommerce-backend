package application

import (
	"context"

	"media-service/internal/domain"
)

type AttachmentFilter struct {
	OwnerService string
	OwnerType    string
	OwnerID      string
	Role         string
}

type MediaRepository interface {
	CreateAsset(ctx context.Context, asset *domain.MediaAsset) error

	GetAssetByID(ctx context.Context, id string) (*domain.MediaAsset, error)

	GetAssetByObjectKey(ctx context.Context, objectKey string) (*domain.MediaAsset, error)

	UpdateAsset(ctx context.Context, asset *domain.MediaAsset) error

	CreateAttachment(ctx context.Context, attachment *domain.MediaAttachment) error

	GetAttachmentByID(ctx context.Context, id string) (*domain.MediaAttachment, error)

	ListAttachments(ctx context.Context, filter AttachmentFilter) ([]*domain.MediaAttachment, error)

	DeleteAttachment(ctx context.Context, id string) error
}
