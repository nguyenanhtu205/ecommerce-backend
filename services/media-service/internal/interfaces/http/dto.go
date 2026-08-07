package httpapi

import "time"

type requestUploadRequest struct {
	MediaType   string  `json:"mediaType"`
	ContentType string  `json:"contentType"`
	Checksum    *string `json:"checksum,omitempty"`
}

type requestUploadResponse struct {
	AssetID       string `json:"assetId"`
	UploadURL     string `json:"uploadUrl"`
	Bucket        string `json:"bucket"`
	ObjectKey     string `json:"objectKey"`
	ExpiresInSecs int    `json:"expiresInSeconds"`
}

type confirmUploadRequest struct {
	Width           *int `json:"width,omitempty"`
	Height          *int `json:"height,omitempty"`
	DurationSeconds *int `json:"durationSeconds,omitempty"`
}

type assetResponse struct {
	ID              string    `json:"id"`
	Bucket          string    `json:"bucket"`
	ObjectKey       string    `json:"objectKey"`
	MediaType       string    `json:"mediaType"`
	ContentType     string    `json:"contentType"`
	SizeBytes       *int64    `json:"sizeBytes,omitempty"`
	Width           *int      `json:"width,omitempty"`
	Height          *int      `json:"height,omitempty"`
	DurationSeconds *int      `json:"durationSeconds,omitempty"`
	Status          string    `json:"status"`
	UploadedBy      string    `json:"uploadedBy"`
	CreatedAt       time.Time `json:"createdAt"`
	PublicURL       string    `json:"publicUrl,omitempty"`
}

type createAttachmentRequest struct {
	MediaAssetID string `json:"mediaAssetId"`
	OwnerService string `json:"ownerService"`
	OwnerType    string `json:"ownerType"`
	OwnerID      string `json:"ownerId"`
	Role         string `json:"role"`
	Position     int    `json:"position"`
}

type attachmentResponse struct {
	ID           string     `json:"id"`
	MediaAssetID string     `json:"mediaAssetId"`
	OwnerService string     `json:"ownerService"`
	OwnerType    string     `json:"ownerType"`
	OwnerID      string     `json:"ownerId"`
	Role         string     `json:"role"`
	Position     int        `json:"position"`
	CreatedAt    time.Time  `json:"createdAt"`
	UpdatedAt    *time.Time `json:"updatedAt,omitempty"`
}

type errorResponse struct {
	Error string `json:"error"`
}
