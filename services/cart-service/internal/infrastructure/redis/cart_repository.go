package redis

import (
	"context"
	"encoding/json"
	"errors"
	"fmt"

	"github.com/redis/go-redis/v9"

	"cart-service/internal/application"
	"cart-service/internal/domain"
)

type CartRepository struct {
	client *redis.Client
}

func NewCartRepository(client *redis.Client) *CartRepository {
	return &CartRepository{client: client}
}

func cartKey(userId string) string {
	return fmt.Sprintf("cart:%s", userId)
}

func (r *CartRepository) AddOrIncrementItem(ctx context.Context, userId, combinationId string, item domain.CartItem) error {
	key := cartKey(userId)
	existingRaw, err := r.client.HGet(ctx, key, combinationId).Result()
	if err != nil && !errors.Is(err, redis.Nil) {
		return err
	}
	if err == nil {
		var existing domain.CartItem
		if uerr := json.Unmarshal([]byte(existingRaw), &existing); uerr != nil {
			return uerr
		}
		existing.Quantity += item.Quantity
		existing.PriceSnapshot = item.PriceSnapshot
		existing.ProductName = item.ProductName
		existing.ThumbnailUrl = item.ThumbnailUrl
		existing.Variation = item.Variation
		existing.ShippingInfo = item.ShippingInfo
		item = existing
	}
	raw, err := json.Marshal(item)
	if err != nil {
		return err
	}
	return r.client.HSet(ctx, key, combinationId, raw).Err()
}

func (r *CartRepository) UpdateItem(ctx context.Context, userId, combinationId string, quantity *int, isSelected *bool) error {
	key := cartKey(userId)
	existingRaw, err := r.client.HGet(ctx, key, combinationId).Result()
	if errors.Is(err, redis.Nil) {
		return application.ErrItemNotFound
	}
	if err != nil {
		return err
	}
	var item domain.CartItem
	if err := json.Unmarshal([]byte(existingRaw), &item); err != nil {
		return err
	}
	if quantity != nil {
		item.Quantity = *quantity
	}
	if isSelected != nil {
		item.IsSelected = *isSelected
	}
	raw, err := json.Marshal(item)
	if err != nil {
		return err
	}
	return r.client.HSet(ctx, key, combinationId, raw).Err()
}

func (r *CartRepository) RemoveItem(ctx context.Context, userId, combinationId string) error {
	return r.client.HDel(ctx, cartKey(userId), combinationId).Err()
}

func (r *CartRepository) RemoveItems(ctx context.Context, userId string, combinationIds []string) error {
	if len(combinationIds) == 0 {
		return nil
	}
	return r.client.HDel(ctx, cartKey(userId), combinationIds...).Err()
}

func (r *CartRepository) GetAll(ctx context.Context, userId string) (map[string]domain.CartItem, error) {
	raw, err := r.client.HGetAll(ctx, cartKey(userId)).Result()
	if err != nil {
		return nil, err
	}
	items := make(map[string]domain.CartItem, len(raw))
	for combinationId, value := range raw {
		var item domain.CartItem
		if err := json.Unmarshal([]byte(value), &item); err != nil {
			return nil, err
		}
		items[combinationId] = item
	}
	return items, nil
}

func (r *CartRepository) Clear(ctx context.Context, userId string) error {
	return r.client.Del(ctx, cartKey(userId)).Err()
}
