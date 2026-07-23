package kafka

import (
	"context"
	"encoding/json"
	"log"
	"time"

	"media-service/internal/application"
	"media-service/internal/domain"

	"github.com/segmentio/kafka-go"
)

type Publisher struct {
	writer *kafka.Writer
}

func NewPublisher(brokers []string, topic string) *Publisher {
	return &Publisher{
		writer: &kafka.Writer{
			Addr:         kafka.TCP(brokers...),
			Topic:        topic,
			Balancer:     &kafka.LeastBytes{},
			RequiredAcks: kafka.RequireOne,
			WriteTimeout: 5 * time.Second,
		},
	}
}

func (p *Publisher) Close() error {
	return p.writer.Close()
}

type envelope struct {
	Event     string    `json:"event"`
	Timestamp time.Time `json:"timestamp"`
	Data      any       `json:"data"`
}

func (p *Publisher) publish(ctx context.Context, key, event string, data any) error {
	payload, err := json.Marshal(envelope{Event: event, Timestamp: time.Now().UTC(), Data: data})
	if err != nil {
		return err
	}

	err = p.writer.WriteMessages(ctx, kafka.Message{
		Key:   []byte(key),
		Value: payload,
	})
	if err != nil {
		log.Printf("Kafka publish failed (event=%s key=%s): %v", event, key, err)
	}
	return err
}

func (p *Publisher) PublishAssetReady(ctx context.Context, asset *domain.MediaAsset) error {
	return p.publish(ctx, asset.ID, application.EventAssetReady, map[string]any{
		"asset_id":    asset.ID,
		"media_type":  asset.MediaType,
		"uploaded_by": asset.UploadedBy,
		"bucket":      asset.Bucket,
		"object_key":  asset.ObjectKey,
	})
}

func (p *Publisher) PublishAssetFailed(ctx context.Context, asset *domain.MediaAsset) error {
	return p.publish(ctx, asset.ID, application.EventAssetFailed, map[string]any{
		"asset_id":   asset.ID,
		"bucket":     asset.Bucket,
		"object_key": asset.ObjectKey,
	})
}

func (p *Publisher) PublishAttachmentCreated(ctx context.Context, attachment *domain.MediaAttachment) error {
	return p.publish(ctx, attachment.MediaAssetID, application.EventAttachmentCreated, map[string]any{
		"attachment_id":  attachment.ID,
		"media_asset_id": attachment.MediaAssetID,
		"owner_service":  attachment.OwnerService,
		"owner_type":     attachment.OwnerType,
		"owner_id":       attachment.OwnerID,
		"role":           attachment.Role,
		"position":       attachment.Position,
	})
}

func (p *Publisher) PublishAttachmentDeleted(ctx context.Context, attachmentID string) error {
	return p.publish(ctx, attachmentID, application.EventAttachmentDeleted, map[string]any{
		"attachment_id": attachmentID,
	})
}
