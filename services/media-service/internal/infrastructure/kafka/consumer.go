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

	ownerServiceReview = "review-service"
	ownerTypeReview    = "review"

	ownerServiceChat = "chat-service"
	ownerTypeChat    = "chat_message"

	ownerServiceUser = "user-service"
	ownerTypeUser    = "user_avatar"
)

type mediaAttachmentItem struct {
	MediaAssetID string `json:"mediaAssetId"`
	Role         string `json:"role"`
	Position     int    `json:"position"`
}

type EventHandler func(ctx context.Context, raw []byte) error

type Consumer struct {
	reader  *kafkago.Reader
	handler EventHandler
}

func NewConsumer(brokers []string, topic, groupID string, handler EventHandler) *Consumer {
	reader := kafkago.NewReader(kafkago.ReaderConfig{
		Brokers:     brokers,
		Topic:       topic,
		GroupID:     groupID,
		StartOffset: kafkago.FirstOffset,
		MinBytes:    1,
		MaxBytes:    10e6,
		MaxWait:     time.Second,
	})
	return &Consumer{reader: reader, handler: handler}
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

		if err := c.handler(ctx, msg.Value); err != nil {
			log.Printf("handle message failed (topic=%s partition=%d offset=%d key=%s): %v",
				c.reader.Config().Topic, msg.Partition, msg.Offset, string(msg.Key), err)
		}

		if err := c.reader.CommitMessages(ctx, msg); err != nil {
			log.Printf("commit offset failed (topic=%s partition=%d offset=%d): %v",
				c.reader.Config().Topic, msg.Partition, msg.Offset, err)
		}
	}
}

func (c *Consumer) Close() error {
	return c.reader.Close()
}

func attachAll(ctx context.Context, svc *application.MediaService, ownerService, ownerType, ownerID string, items []mediaAttachmentItem) error {
	var firstErr error
	for _, item := range items {
		_, err := svc.CreateAttachment(ctx, application.CreateAttachmentInput{
			MediaAssetID: item.MediaAssetID,
			OwnerService: ownerService,
			OwnerType:    ownerType,
			OwnerID:      ownerID,
			Role:         item.Role,
			Position:     item.Position,
		})
		if err != nil && firstErr == nil {
			firstErr = fmt.Errorf("attach media_asset_id=%s role=%s: %w", item.MediaAssetID, item.Role, err)
		}
	}
	return firstErr
}

type productMediaAttachedEvent struct {
	ProductID        string                `json:"productId"`
	ShopID           string                `json:"shopId"`
	MediaAttachments []mediaAttachmentItem `json:"mediaAttachments"`
	OccurredAt       time.Time             `json:"occurredAt"`
}

func NewProductMediaAttachedHandler(svc *application.MediaService) EventHandler {
	return func(ctx context.Context, raw []byte) error {
		var evt productMediaAttachedEvent
		if err := json.Unmarshal(raw, &evt); err != nil {
			return fmt.Errorf("unmarshal event: %w", err)
		}
		if evt.ProductID == "" {
			return fmt.Errorf("event missing productId")
		}
		return attachAll(ctx, svc, ownerServiceProductCatalog, ownerTypeProduct, evt.ProductID, evt.MediaAttachments)
	}
}

type reviewMediaAttachedEvent struct {
	ReviewID         string                `json:"reviewId"`
	BuyerID          string                `json:"buyerId"`
	MediaAttachments []mediaAttachmentItem `json:"mediaAttachments"`
	OccurredAt       time.Time             `json:"occurredAt"`
}

func NewReviewMediaAttachedHandler(svc *application.MediaService) EventHandler {
	return func(ctx context.Context, raw []byte) error {
		var evt reviewMediaAttachedEvent
		if err := json.Unmarshal(raw, &evt); err != nil {
			return fmt.Errorf("unmarshal event: %w", err)
		}
		if evt.ReviewID == "" {
			return fmt.Errorf("event missing reviewId")
		}
		return attachAll(ctx, svc, ownerServiceReview, ownerTypeReview, evt.ReviewID, evt.MediaAttachments)
	}
}

type chatMediaAttachedEvent struct {
	MessageID        string                `json:"messageId"`
	OwnerID          string                `json:"ownerId"`
	MediaAttachments []mediaAttachmentItem `json:"mediaAttachments"`
	OccurredAt       time.Time             `json:"occurredAt"`
}

func NewChatMediaAttachedHandler(svc *application.MediaService) EventHandler {
	return func(ctx context.Context, raw []byte) error {
		var evt chatMediaAttachedEvent
		if err := json.Unmarshal(raw, &evt); err != nil {
			return fmt.Errorf("unmarshal event: %w", err)
		}
		if evt.MessageID == "" {
			return fmt.Errorf("event missing messageId")
		}
		return attachAll(ctx, svc, ownerServiceChat, ownerTypeChat, evt.MessageID, evt.MediaAttachments)
	}
}

type avatarMediaAttachedEvent struct {
	UserID           string                `json:"userId"`
	MediaAttachments []mediaAttachmentItem `json:"mediaAttachments"`
	OccurredAt       time.Time             `json:"occurredAt"`
}

func NewAvatarMediaAttachedHandler(svc *application.MediaService) EventHandler {
	return func(ctx context.Context, raw []byte) error {
		var evt avatarMediaAttachedEvent
		if err := json.Unmarshal(raw, &evt); err != nil {
			return fmt.Errorf("unmarshal event: %w", err)
		}
		if evt.UserID == "" {
			return fmt.Errorf("event missing userId")
		}
		return attachAll(ctx, svc, ownerServiceUser, ownerTypeUser, evt.UserID, evt.MediaAttachments)
	}
}
