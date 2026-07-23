package domain

import "time"

type MediaType string

const (
	MediaTypeImage MediaType = "IMAGE"
	MediaTypeVideo MediaType = "VIDEO"
)

type MediaStatus string

const (
	StatusPendingUpload MediaStatus = "PENDING_UPLOAD"
	StatusProcessing    MediaStatus = "PROCESSING"
	StatusReady         MediaStatus = "READY"
	StatusFailed        MediaStatus = "FAILED"
	StatusDeleting      MediaStatus = "DELETING"
	StatusDeleted       MediaStatus = "DELETED"
)

type MediaAsset struct {
	ID              string
	Bucket          string
	ObjectKey       string
	MediaType       MediaType
	ContentType     string
	SizeBytes       *int64
	Width           *int
	Height          *int
	DurationSeconds *int
	Status          MediaStatus
	Checksum        *string
	UploadedBy      string
	CreatedAt       time.Time
}
