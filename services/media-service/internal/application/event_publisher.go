package application

import (
	"context"

	"media-service/internal/domain"
)

const (
	EventAssetReady        = "media.asset.ready"
	EventAssetFailed       = "media.asset.failed"
	EventAttachmentCreated = "media.attachment.created"
	EventAttachmentDeleted = "media.attachment.deleted"
)

type EventPublisher interface {
	PublishAssetReady(ctx context.Context, asset *domain.MediaAsset) error
	PublishAssetFailed(ctx context.Context, asset *domain.MediaAsset) error
	PublishAttachmentCreated(ctx context.Context, attachment *domain.MediaAttachment) error
	PublishAttachmentDeleted(ctx context.Context, attachmentID string) error
}
