package mongodb

import (
	"context"
	"errors"
	"time"

	"github.com/google/uuid"
	"go.mongodb.org/mongo-driver/v2/bson"
	"go.mongodb.org/mongo-driver/v2/mongo"
	"go.mongodb.org/mongo-driver/v2/mongo/options"

	"chat-service/internal/application"
	"chat-service/internal/domain"
)

type ChatRepository struct {
	conversations *mongo.Collection
	messages      *mongo.Collection
}

func NewChatRepository(db *mongo.Database) *ChatRepository {
	return &ChatRepository{
		conversations: db.Collection("conversations"),
		messages:      db.Collection("messages"),
	}
}

func (r *ChatRepository) EnsureIndexes(ctx context.Context) error {
	_, err := r.conversations.Indexes().CreateMany(ctx, []mongo.IndexModel{
		{
			Keys:    bson.D{{Key: "buyerId", Value: 1}, {Key: "shopId", Value: 1}},
			Options: options.Index().SetUnique(true),
		},
		{Keys: bson.D{{Key: "buyerId", Value: 1}}},
		{Keys: bson.D{{Key: "shopId", Value: 1}}},
	})
	if err != nil {
		return err
	}

	_, err = r.messages.Indexes().CreateOne(ctx, mongo.IndexModel{
		Keys: bson.D{{Key: "conversationId", Value: 1}, {Key: "createdAt", Value: -1}},
	})
	return err
}

func (r *ChatRepository) GetOrCreateConversation(ctx context.Context, buyerID, shopID string) (*domain.Conversation, error) {
	filter := bson.M{"buyerId": buyerID, "shopId": shopID}

	var existing domain.Conversation
	err := r.conversations.FindOne(ctx, filter).Decode(&existing)
	if err == nil {
		return &existing, nil
	}
	if !errors.Is(err, mongo.ErrNoDocuments) {
		return nil, err
	}

	conv := &domain.Conversation{
		ID:        uuid.NewString(),
		BuyerID:   buyerID,
		ShopID:    shopID,
		CreatedAt: time.Now().UTC(),
	}
	if _, err := r.conversations.InsertOne(ctx, conv); err != nil {
		if mongo.IsDuplicateKeyError(err) {
			var existing2 domain.Conversation
			if ferr := r.conversations.FindOne(ctx, filter).Decode(&existing2); ferr != nil {
				return nil, ferr
			}
			return &existing2, nil
		}
		return nil, err
	}
	return conv, nil
}

func (r *ChatRepository) GetConversationByID(ctx context.Context, id string) (*domain.Conversation, error) {
	var conv domain.Conversation
	err := r.conversations.FindOne(ctx, bson.M{"_id": id}).Decode(&conv)
	if err != nil {
		if errors.Is(err, mongo.ErrNoDocuments) {
			return nil, application.ErrConversationNotFound
		}
		return nil, err
	}
	return &conv, nil
}

func (r *ChatRepository) InsertMessage(ctx context.Context, msg *domain.Message) error {
	if msg.ID == "" {
		msg.ID = uuid.NewString()
	}
	if msg.CreatedAt.IsZero() {
		msg.CreatedAt = time.Now().UTC()
	}
	_, err := r.messages.InsertOne(ctx, msg)
	return err
}

func (r *ChatRepository) UpdateConversationOnNewMessage(ctx context.Context, conversationID, lastMessage string, sender domain.SenderType) (*domain.Conversation, error) {
	unreadField := "sellerUnreadCount"
	if sender == domain.SenderShop {
		unreadField = "buyerUnreadCount"
	}

	after := options.After
	var updated domain.Conversation
	err := r.conversations.FindOneAndUpdate(ctx,
		bson.M{"_id": conversationID},
		bson.M{
			"$set": bson.M{"lastMessage": lastMessage, "lastMessageAt": time.Now().UTC(), "lastMessageSenderType": sender},
			"$inc": bson.M{unreadField: 1},
		},
		options.FindOneAndUpdate().SetReturnDocument(after),
	).Decode(&updated)
	if err != nil {
		return nil, err
	}
	return &updated, nil
}

func (r *ChatRepository) ListConversations(ctx context.Context, userID string, role domain.SenderType) ([]*domain.Conversation, error) {
	field := "buyerId"
	if role == domain.SenderShop {
		field = "shopId"
	}

	opts := options.Find().SetSort(bson.D{{Key: "lastMessageAt", Value: -1}})
	cursor, err := r.conversations.Find(ctx, bson.M{field: userID}, opts)
	if err != nil {
		return nil, err
	}
	defer func(cursor *mongo.Cursor, ctx context.Context) {
		err := cursor.Close(ctx)
		if err != nil {
		}
	}(cursor, ctx)

	var result []*domain.Conversation
	if err := cursor.All(ctx, &result); err != nil {
		return nil, err
	}
	return result, nil
}

func (r *ChatRepository) GetMessageHistory(ctx context.Context, conversationID string, page, pageSize int) ([]*domain.Message, error) {
	if page < 1 {
		page = 1
	}
	if pageSize < 1 {
		pageSize = 20
	}

	opts := options.Find().
		SetSort(bson.D{{Key: "createdAt", Value: -1}}).
		SetSkip(int64((page - 1) * pageSize)).
		SetLimit(int64(pageSize))

	cursor, err := r.messages.Find(ctx, bson.M{"conversationId": conversationID}, opts)
	if err != nil {
		return nil, err
	}
	defer func(cursor *mongo.Cursor, ctx context.Context) {
		err := cursor.Close(ctx)
		if err != nil {
		}
	}(cursor, ctx)

	result := make([]*domain.Message, 0)
	if err := cursor.All(ctx, &result); err != nil {
		return nil, err
	}
	return result, nil
}

func (r *ChatRepository) MarkAsRead(ctx context.Context, conversationID string, reader domain.SenderType) error {
	senderOfUnreadMsgs := domain.SenderShop
	unreadField := "buyerUnreadCount"
	if reader == domain.SenderShop {
		senderOfUnreadMsgs = domain.SenderBuyer
		unreadField = "sellerUnreadCount"
	}

	if _, err := r.messages.UpdateMany(ctx,
		bson.M{"conversationId": conversationID, "senderType": senderOfUnreadMsgs, "isRead": false},
		bson.M{"$set": bson.M{"isRead": true}},
	); err != nil {
		return err
	}

	_, err := r.conversations.UpdateOne(ctx,
		bson.M{"_id": conversationID},
		bson.M{"$set": bson.M{unreadField: 0}},
	)
	return err
}

var _ application.ChatRepository = (*ChatRepository)(nil)
