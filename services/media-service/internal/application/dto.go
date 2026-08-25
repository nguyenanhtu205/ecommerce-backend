package application

import "media-service/internal/domain"

type RequestUploadInput struct {
	MediaType   domain.MediaType
	ContentType string
	UploadedBy  string
	Checksum    *string
}

type RequestUploadOutput struct {
	AssetID       string
	UploadURL     string
	Bucket        string
	ObjectKey     string
	ExpiresInSecs int
}

type ConfirmUploadInput struct {
	AssetID         string
	Width           *int
	Height          *int
	DurationSeconds *int
}

type CreateAttachmentInput struct {
	MediaAssetID string
	OwnerService string
	OwnerType    string
	OwnerID      string
	Role         string
	Position     int
}

type AssetResult struct {
	Asset     *domain.MediaAsset
	PublicURL string
}

type OwnerRoleAssetResult struct {
	OwnerID   string
	Role      string
	Found     bool
	Asset     *domain.MediaAsset
	PublicURL string
}
