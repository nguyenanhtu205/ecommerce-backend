package httpapi

import "time"

type requestUploadRequest struct {
	MediaType   string  `json:"media_type"`
	ContentType string  `json:"content_type"`
	UploadedBy  string  `json:"uploaded_by"`
	Checksum    *string `json:"checksum,omitempty"`
}

type requestUploadResponse struct {
	AssetID       string `json:"asset_id"`
	UploadURL     string `json:"upload_url"`
	Bucket        string `json:"bucket"`
	ObjectKey     string `json:"object_key"`
	ExpiresInSecs int    `json:"expires_in_seconds"`
}

type confirmUploadRequest struct {
	Width           *int `json:"width,omitempty"`
	Height          *int `json:"height,omitempty"`
	DurationSeconds *int `json:"duration_seconds,omitempty"`
}

type assetResponse struct {
	ID              string    `json:"id"`
	Bucket          string    `json:"bucket"`
	ObjectKey       string    `json:"object_key"`
	MediaType       string    `json:"media_type"`
	ContentType     string    `json:"content_type"`
	SizeBytes       *int64    `json:"size_bytes,omitempty"`
	Width           *int      `json:"width,omitempty"`
	Height          *int      `json:"height,omitempty"`
	DurationSeconds *int      `json:"duration_seconds,omitempty"`
	Status          string    `json:"status"`
	UploadedBy      string    `json:"uploaded_by"`
	CreatedAt       time.Time `json:"created_at"`
	PublicURL       string    `json:"public_url,omitempty"`
}

type createAttachmentRequest struct {
	MediaAssetID string `json:"media_asset_id"`
	OwnerService string `json:"owner_service"`
	OwnerType    string `json:"owner_type"`
	OwnerID      string `json:"owner_id"`
	Role         string `json:"role"`
	Position     int    `json:"position"`
}

type attachmentResponse struct {
	ID           string     `json:"id"`
	MediaAssetID string     `json:"media_asset_id"`
	OwnerService string     `json:"owner_service"`
	OwnerType    string     `json:"owner_type"`
	OwnerID      string     `json:"owner_id"`
	Role         string     `json:"role"`
	Position     int        `json:"position"`
	CreatedAt    time.Time  `json:"created_at"`
	UpdatedAt    *time.Time `json:"updated_at,omitempty"`
}

type errorResponse struct {
	Error string `json:"error"`
}
