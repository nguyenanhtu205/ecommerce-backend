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
