package mongodb

import (
	"context"
	"errors"
	"time"

	"go.mongodb.org/mongo-driver/v2/bson"
	"go.mongodb.org/mongo-driver/v2/mongo"
	"go.mongodb.org/mongo-driver/v2/mongo/options"

	"chat-service/internal/application"
	"chat-service/internal/domain"
)

type ShopChatSettingsRepository struct {
	collection *mongo.Collection
}

func NewShopChatSettingsRepository(db *mongo.Database) *ShopChatSettingsRepository {
	return &ShopChatSettingsRepository{collection: db.Collection("shop_chat_settings")}
}

func (r *ShopChatSettingsRepository) UpsertChatSettings(ctx context.Context, shopID string, autoReplyEnabled bool, autoReplyMessage string) error {
	_, err := r.collection.UpdateOne(ctx,
		bson.M{"_id": shopID},
		bson.M{"$set": bson.M{
			"autoReplyEnabled": autoReplyEnabled,
			"autoReplyMessage": autoReplyMessage,
			"updatedAt":        time.Now().UTC(),
		}},
		options.UpdateOne().SetUpsert(true),
	)
	return err
}

func (r *ShopChatSettingsRepository) UpsertVacationSettings(ctx context.Context, shopID string, enabled bool, startDate, endDate *time.Time, message string) error {
	_, err := r.collection.UpdateOne(ctx,
		bson.M{"_id": shopID},
		bson.M{"$set": bson.M{
			"vacationEnabled":   enabled,
			"vacationStartDate": startDate,
			"vacationEndDate":   endDate,
			"vacationMessage":   message,
			"updatedAt":         time.Now().UTC(),
		}},
		options.UpdateOne().SetUpsert(true),
	)
	return err
}

func (r *ShopChatSettingsRepository) GetByShopID(ctx context.Context, shopID string) (*domain.ShopChatSettings, error) {
	var settings domain.ShopChatSettings
	err := r.collection.FindOne(ctx, bson.M{"_id": shopID}).Decode(&settings)
	if err != nil {
		if errors.Is(err, mongo.ErrNoDocuments) {
			return nil, nil
		}
		return nil, err
	}
	return &settings, nil
}

var _ application.ShopChatSettingsRepository = (*ShopChatSettingsRepository)(nil)
