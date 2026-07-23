package application

import (
	"context"
	"time"
)

type ObjectStorage interface {
	GeneratePresignedUploadURL(ctx context.Context, bucket, objectKey, contentType string, expiry time.Duration) (string, error)

	HeadObject(ctx context.Context, bucket, objectKey string) (*ObjectInfo, error)

	GetPublicURL(bucket, objectKey string) string

	DeleteObject(ctx context.Context, bucket, objectKey string) error
}

type ObjectInfo struct {
	SizeBytes   int64
	ContentType string
}
