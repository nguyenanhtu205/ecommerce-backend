package minio

import (
	"context"
	"fmt"
	"io"
	"time"

	"github.com/minio/minio-go/v7"
	"github.com/minio/minio-go/v7/pkg/credentials"

	"media-service/internal/application"
)

type ObjectStorage struct {
	client   *minio.Client
	endpoint string
	useSSL   bool
}

func NewObjectStorage(endpoint, accessKey, secretKey string, useSSL bool) (*ObjectStorage, error) {
	client, err := minio.New(endpoint, &minio.Options{
		Creds:  credentials.NewStaticV4(accessKey, secretKey, ""),
		Secure: useSSL,
	})
	if err != nil {
		return nil, fmt.Errorf("init minio client: %w", err)
	}
	return &ObjectStorage{client: client, endpoint: endpoint, useSSL: useSSL}, nil
}

func (s *ObjectStorage) GeneratePresignedUploadURL(ctx context.Context, bucket, objectKey, _ string, expiry time.Duration) (string, error) {
	u, err := s.client.PresignedPutObject(ctx, bucket, objectKey, expiry)
	if err != nil {
		return "", err
	}
	return u.String(), nil
}

func (s *ObjectStorage) HeadObject(ctx context.Context, bucket, objectKey string) (*application.ObjectInfo, error) {
	info, err := s.client.StatObject(ctx, bucket, objectKey, minio.StatObjectOptions{})
	if err != nil {
		return nil, err
	}
	return &application.ObjectInfo{
		SizeBytes:   info.Size,
		ContentType: info.ContentType,
	}, nil
}

func (s *ObjectStorage) GetPublicURL(bucket, objectKey string) string {
	scheme := "http"
	if s.useSSL {
		scheme = "https"
	}
	return fmt.Sprintf("%s://%s/%s/%s", scheme, s.endpoint, bucket, objectKey)
}

func (s *ObjectStorage) DeleteObject(ctx context.Context, bucket, objectKey string) error {
	return s.client.RemoveObject(ctx, bucket, objectKey, minio.RemoveObjectOptions{})
}

func (s *ObjectStorage) GetObjectStream(ctx context.Context, bucket, objectKey string) (io.ReadCloser, error) {
	return s.client.GetObject(ctx, bucket, objectKey, minio.GetObjectOptions{})
}
