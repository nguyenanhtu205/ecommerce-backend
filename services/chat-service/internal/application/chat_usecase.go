package application

import (
	"context"
	"encoding/json"
	"log"

	"chat-service/internal/domain"
)

type ChatUseCase struct {
	repo      ChatRepository
	publisher EventPublisher
	pusher    RealtimePusher
}

func NewChatUseCase(repo ChatRepository, publisher EventPublisher, pusher RealtimePusher) *ChatUseCase {
	return &ChatUseCase{repo: repo, publisher: publisher, pusher: pusher}
}

func (uc *ChatUseCase) GetOrCreateConversation(ctx context.Context, buyerID, shopID string) (*domain.Conversation, error) {
	return uc.repo.GetOrCreateConversation(ctx, buyerID, shopID)
}

func (uc *ChatUseCase) ListConversations(ctx context.Context, userID string, role domain.SenderType) ([]*domain.Conversation, error) {
	return uc.repo.ListConversations(ctx, userID, role)
}

func (uc *ChatUseCase) GetMessageHistory(ctx context.Context, conversationID string, page int) ([]*domain.Message, error) {
	const pageSize = 20
	return uc.repo.GetMessageHistory(ctx, conversationID, page, pageSize)
}

func (uc *ChatUseCase) MarkAsRead(ctx context.Context, conversationID string, reader domain.SenderType) error {
	return uc.repo.MarkAsRead(ctx, conversationID, reader)
}

type SendMessageInput struct {
	ConversationID string
	SenderType     domain.SenderType
	Content        string
	AttachmentIDs  []string
}

func (uc *ChatUseCase) SendMessage(ctx context.Context, in SendMessageInput) (*domain.Message, error) {
	msg := &domain.Message{
		ConversationID:          in.ConversationID,
		SenderType:              in.SenderType,
		Content:                 in.Content,
		AttachmentMediaAssetIDs: in.AttachmentIDs,
		IsRead:                  false,
	}

	if err := uc.repo.InsertMessage(ctx, msg); err != nil {
		return nil, err
	}

	conv, err := uc.repo.UpdateConversationOnNewMessage(ctx, in.ConversationID, in.Content, in.SenderType)
	if err != nil {
		return nil, err
	}

	if uc.publisher != nil {
		event := MessageSentEvent{
			MessageID:      msg.ID,
			ConversationID: msg.ConversationID,
			SenderType:     string(msg.SenderType),
			Content:        msg.Content,
			AttachmentIDs:  msg.AttachmentMediaAssetIDs,
			BuyerID:        conv.BuyerID,
			ShopID:         conv.ShopID,
		}
		if err := uc.publisher.PublishMessageSent(ctx, event); err != nil {
			log.Printf("chat-service: publish MessageSent event failed: %v", err)
		}
	}

	if uc.pusher != nil {
		if payload, err := json.Marshal(msg); err == nil {
			uc.pusher.PushToConversation(msg.ConversationID, payload)
		}
	}

	return msg, nil
}
