package application

import (
	"context"
	"encoding/json"
	"fmt"
	"time"
)

type ShopChatSettingsUpdatedEvent struct {
	ShopID           string `json:"shopId"`
	AutoReplyEnabled bool   `json:"autoReplyEnabled"`
	AutoReplyMessage string `json:"autoReplyMessage"`
}

type ShopVacationSettingsUpdatedEvent struct {
	ShopID    string  `json:"shopId"`
	IsEnabled bool    `json:"isEnabled"`
	StartDate *string `json:"startDate"`
	EndDate   *string `json:"endDate"`
	Message   string  `json:"message"`
}

type ShopChatSettingsIngestUseCase struct {
	repo ShopChatSettingsRepository
}

func NewShopChatSettingsIngestUseCase(repo ShopChatSettingsRepository) *ShopChatSettingsIngestUseCase {
	return &ShopChatSettingsIngestUseCase{repo: repo}
}

func (uc *ShopChatSettingsIngestUseCase) HandleChatSettingsUpdated(ctx context.Context, raw []byte) error {
	var event ShopChatSettingsUpdatedEvent
	if err := json.Unmarshal(raw, &event); err != nil {
		return fmt.Errorf("unmarshal ShopChatSettingsUpdated event: %w", err)
	}
	if event.ShopID == "" {
		return fmt.Errorf("invalid ShopChatSettingsUpdated event: missing shopId")
	}

	return uc.repo.UpsertChatSettings(ctx, event.ShopID, event.AutoReplyEnabled, event.AutoReplyMessage)
}

func (uc *ShopChatSettingsIngestUseCase) HandleVacationSettingsUpdated(ctx context.Context, raw []byte) error {
	var event ShopVacationSettingsUpdatedEvent
	if err := json.Unmarshal(raw, &event); err != nil {
		return fmt.Errorf("unmarshal ShopVacationSettingsUpdated event: %w", err)
	}
	if event.ShopID == "" {
		return fmt.Errorf("invalid ShopVacationSettingsUpdated event: missing shopId")
	}

	startDate, err := parseDatePtr(event.StartDate)
	if err != nil {
		return fmt.Errorf("parse startDate: %w", err)
	}
	endDate, err := parseDatePtr(event.EndDate)
	if err != nil {
		return fmt.Errorf("parse endDate: %w", err)
	}

	return uc.repo.UpsertVacationSettings(ctx, event.ShopID, event.IsEnabled, startDate, endDate, event.Message)
}

func parseDatePtr(s *string) (*time.Time, error) {
	if s == nil || *s == "" {
		return nil, nil
	}
	t, err := time.Parse("2006-01-02", *s)
	if err != nil {
		return nil, err
	}
	return &t, nil
}
