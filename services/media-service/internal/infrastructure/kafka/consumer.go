package kafka

import (
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"log"
	"time"

	kafkago "github.com/segmentio/kafka-go"

	"media-service/internal/application"
)

const (
	ownerServiceProductCatalog = "product-catalog-service"
	ownerTypeProduct           = "product"
)

type Consumer struct {
	reader *kafkago.Reader
	svc    *application.MediaService
}

func NewConsumer(brokers []string, topic string, groupID string, svc *application.MediaService) *Consumer {
	reader := kafkago.NewReader(kafkago.ReaderConfig{
		Brokers:     brokers,
		Topic:       topic,
		GroupID:     groupID,
		StartOffset: kafkago.FirstOffset,
		MinBytes:    1,
		MaxBytes:    10e6,
		MaxWait:     time.Second,
	})

	return &Consumer{reader: reader, svc: svc}
}

func (c *Consumer) Start(ctx context.Context) error {
	for {
		msg, err := c.reader.FetchMessage(ctx)
		if err != nil {
			if errors.Is(err, context.Canceled) {
				return nil
			}
			return fmt.Errorf("fetch message: %w", err)
		}

		if err := c.handleProductMediaAttached(ctx, msg.Value); err != nil {
			log.Printf("handle message failed (partition=%d offset=%d key=%s): %v",
				msg.Partition, msg.Offset, string(msg.Key), err)
		}

		if err := c.reader.CommitMessages(ctx, msg); err != nil {
			log.Printf("commit offset failed (partition=%d offset=%d): %v",
				msg.Partition, msg.Offset, err)
		}
	}
}

func (c *Consumer) Close() error {
	return c.reader.Close()
}

type productMediaAttachedEvent struct {
	ProductID        string                `json:"productId"`
	ShopID           string                `json:"shopId"`
	MediaAttachments []mediaAttachmentItem `json:"mediaAttachments"`
	OccurredAt       time.Time             `json:"occurredAt"`
}

type mediaAttachmentItem struct {
	MediaAssetID string `json:"mediaAssetId"`
	Role         string `json:"role"`
	Position     int    `json:"position"`
}

func (c *Consumer) handleProductMediaAttached(ctx context.Context, raw []byte) error {
	var evt productMediaAttachedEvent
	if err := json.Unmarshal(raw, &evt); err != nil {
		return fmt.Errorf("unmarshal event: %w", err)
	}
	if evt.ProductID == "" {
		return fmt.Errorf("event missing productId")
	}

	var firstErr error
	for _, item := range evt.MediaAttachments {
		_, err := c.svc.CreateAttachment(ctx, application.CreateAttachmentInput{
			MediaAssetID: item.MediaAssetID,
			OwnerService: ownerServiceProductCatalog,
			OwnerType:    ownerTypeProduct,
			OwnerID:      evt.ProductID,
			Role:         item.Role,
			Position:     item.Position,
		})
		if err != nil && firstErr == nil {
			firstErr = fmt.Errorf("attach media_asset_id=%s role=%s: %w", item.MediaAssetID, item.Role, err)
		}
	}
	return firstErr
}
