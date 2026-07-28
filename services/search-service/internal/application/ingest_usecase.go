package application

import (
	"context"
	"encoding/json"
	"fmt"

	"search-service/internal/domain"
)

type IngestUseCase struct {
	repo SearchRepository
}

func NewIngestUseCase(repo SearchRepository) *IngestUseCase {
	return &IngestUseCase{repo: repo}
}

func (uc *IngestUseCase) HandleProductListingViewUpdated(ctx context.Context, raw []byte) error {
	var listing domain.ProductListing
	if err := json.Unmarshal(raw, &listing); err != nil {
		return fmt.Errorf("unmarshal ProductListingViewUpdated: %w", err)
	}

	if listing.ProductID == "" || listing.SyncedAt == "" {
		return fmt.Errorf("invalid event: missing productId or syncedAt")
	}

	if err := uc.repo.Upsert(ctx, listing); err != nil {
		return fmt.Errorf("upsert product listing %s: %w", listing.ProductID, err)
	}

	return nil
}
