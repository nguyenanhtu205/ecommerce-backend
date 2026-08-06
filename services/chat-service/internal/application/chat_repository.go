package application

import (
	"context"

	"chat-service/internal/domain"
)

type ChatRepository interface {
	GetOrCreateConversation(ctx context.Context, buyerID, shopID string) (*domain.Conversation, error)

	InsertMessage(ctx context.Context, msg *domain.Message) error

	UpdateConversationOnNewMessage(ctx context.Context, conversationID, lastMessage string, sender domain.SenderType) (*domain.Conversation, error)

	ListConversations(ctx context.Context, userID string, role domain.SenderType) ([]*domain.Conversation, error)

	GetMessageHistory(ctx context.Context, conversationID string, page, pageSize int) ([]*domain.Message, error)

	MarkAsRead(ctx context.Context, conversationID string, reader domain.SenderType) error
}
