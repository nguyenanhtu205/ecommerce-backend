package application

import "context"

type MessageSentEvent struct {
	MessageID      string   `json:"messageId"`
	ConversationID string   `json:"conversationId"`
	SenderType     string   `json:"senderType"`
	Content        string   `json:"content"`
	AttachmentIDs  []string `json:"attachmentMediaAssetIds"`
	BuyerID        string   `json:"buyerId"`
	ShopID         string   `json:"shopId"`
}

type EventPublisher interface {
	PublishMessageSent(ctx context.Context, event MessageSentEvent) error
}

type RealtimePusher interface {
	PushToConversation(conversationID string, payload []byte)
}
