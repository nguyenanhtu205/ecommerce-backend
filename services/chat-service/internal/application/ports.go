package application

import (
	"context"
	"time"
)

type MessageSentEvent struct {
	MessageID      string   `json:"messageId"`
	ConversationID string   `json:"conversationId"`
	SenderType     string   `json:"senderType"`
	Content        string   `json:"content"`
	AttachmentIDs  []string `json:"attachmentMediaAssetIds"`
	BuyerID        string   `json:"buyerId"`
	ShopID         string   `json:"shopId"`
}

type MediaAttachmentItem struct {
	MediaAssetID string `json:"mediaAssetId"`
	Role         string `json:"role"`
	Position     int    `json:"position"`
}

type ChatMediaAttachedEvent struct {
	MessageID        string                `json:"messageId"`
	OwnerID          string                `json:"ownerId"`
	MediaAttachments []MediaAttachmentItem `json:"mediaAttachments"`
	OccurredAt       time.Time             `json:"occurredAt"`
}

type EventPublisher interface {
	PublishMessageSent(ctx context.Context, event MessageSentEvent) error
	PublishMediaAttached(ctx context.Context, event ChatMediaAttachedEvent) error
}

type RealtimePusher interface {
	PushToConversation(conversationID string, payload []byte)
}
