package application

import (
	"context"
	"errors"

	"cart-service/internal/domain"
)

var ErrItemNotFound = errors.New("cart item not found")

type CartRepository interface {
	AddOrIncrementItem(ctx context.Context, userId, combinationId string, item domain.CartItem) error

	UpdateItem(ctx context.Context, userId, combinationId string, quantity *int, isSelected *bool) error

	RemoveItem(ctx context.Context, userId, combinationId string) error

	RemoveItems(ctx context.Context, userId string, combinationIds []string) error

	GetAll(ctx context.Context, userId string) (map[string]domain.CartItem, error)

	Clear(ctx context.Context, userId string) error
}
