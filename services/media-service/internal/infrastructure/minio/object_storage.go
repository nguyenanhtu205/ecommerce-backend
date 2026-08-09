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
	internalClient *minio.Client
	publicClient   *minio.Client
	publicEndpoint string
	publicUseSSL   bool
}

func NewObjectStorage(internalEndpoint, publicEndpoint, accessKey, secretKey string, internalUseSSL, publicUseSSL bool) (*ObjectStorage, error) {
	creds := credentials.NewStaticV4(accessKey, secretKey, "")

	internalClient, err := minio.New(internalEndpoint, &minio.Options{
		Creds:  creds,
		Secure: internalUseSSL,
		Region: "us-east-1",
	})
	if err != nil {
		return nil, fmt.Errorf("init internal minio client: %w", err)
	}

	publicClient, err := minio.New(publicEndpoint, &minio.Options{
		Creds:  creds,
		Secure: publicUseSSL,
		Region: "us-east-1",
	})
	if err != nil {
		return nil, fmt.Errorf("init public minio client: %w", err)
	}

	return &ObjectStorage{
		internalClient: internalClient,
		publicClient:   publicClient,
		publicEndpoint: publicEndpoint,
		publicUseSSL:   publicUseSSL,
	}, nil
}

func (s *ObjectStorage) GeneratePresignedUploadURL(ctx context.Context, bucket, objectKey, _ string, expiry time.Duration) (string, error) {
	u, err := s.publicClient.PresignedPutObject(ctx, bucket, objectKey, expiry)
	if err != nil {
		return "", err
	}
	return u.String(), nil
}

func (s *ObjectStorage) HeadObject(ctx context.Context, bucket, objectKey string) (*application.ObjectInfo, error) {
	info, err := s.internalClient.StatObject(ctx, bucket, objectKey, minio.StatObjectOptions{})
	if err != nil {
		return nil, err
	}
	return &application.ObjectInfo{
		SizeBytes:   info.Size,
		ContentType: info.ContentType,
	}, nil
}

func (s *ObjectStorage) GetObjectStream(ctx context.Context, bucket, objectKey string) (io.ReadCloser, error) {
	return s.internalClient.GetObject(ctx, bucket, objectKey, minio.GetObjectOptions{})
}

func (s *ObjectStorage) GetPublicURL(bucket, objectKey string) string {
	scheme := "http"
	if s.publicUseSSL {
		scheme = "https"
	}
	return fmt.Sprintf("%s://%s/%s/%s", scheme, s.publicEndpoint, bucket, objectKey)
}

func (s *ObjectStorage) DeleteObject(ctx context.Context, bucket, objectKey string) error {
	return s.internalClient.RemoveObject(ctx, bucket, objectKey, minio.RemoveObjectOptions{})
}
