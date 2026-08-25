package application

import (
	"context"
	"time"

	"chat-service/internal/domain"
)

type ShopChatSettingsRepository interface {
	UpsertChatSettings(ctx context.Context, shopID string, autoReplyEnabled bool, autoReplyMessage string) error

	UpsertVacationSettings(ctx context.Context, shopID string, enabled bool, startDate, endDate *time.Time, message string) error

	GetByShopID(ctx context.Context, shopID string) (*domain.ShopChatSettings, error)
}
