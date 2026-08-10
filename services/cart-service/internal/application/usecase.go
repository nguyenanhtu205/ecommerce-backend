package application

import (
	"context"
	"errors"
	"sort"
	"time"

	"cart-service/internal/domain"
)

type CartUseCase struct {
	repo CartRepository
}

func NewCartUseCase(repo CartRepository) *CartUseCase {
	return &CartUseCase{repo: repo}
}

func (uc *CartUseCase) AddItem(ctx context.Context, userId string, req AddItemRequest) error {
	item := domain.CartItem{
		ProductId:     req.ProductId,
		ShopId:        req.ShopId,
		ShopName:      req.ShopName,
		ProductName:   req.ProductName,
		ThumbnailUrl:  req.ThumbnailUrl,
		Variation:     req.Variation,
		Quantity:      req.Quantity,
		IsSelected:    req.IsSelected,
		PriceSnapshot: req.PriceSnapshot,
		AddedAt:       time.Now().UTC().Format(time.RFC3339),
	}
	return uc.repo.AddOrIncrementItem(ctx, userId, req.CombinationId, item)
}

func (uc *CartUseCase) UpdateItem(ctx context.Context, userId, combinationId string, req UpdateItemRequest) error {
	if req.Quantity == nil && req.IsSelected == nil {
		return errors.New("at least one of quantity or isSelected must be provided")
	}
	return uc.repo.UpdateItem(ctx, userId, combinationId, req.Quantity, req.IsSelected)
}

func (uc *CartUseCase) RemoveItem(ctx context.Context, userId, combinationId string) error {
	return uc.repo.RemoveItem(ctx, userId, combinationId)
}

func (uc *CartUseCase) RemoveItems(ctx context.Context, userId string, combinationIds []string) error {
	return uc.repo.RemoveItems(ctx, userId, combinationIds)
}

func (uc *CartUseCase) GetCart(ctx context.Context, userId string) ([]ShopCartGroup, error) {
	items, err := uc.repo.GetAll(ctx, userId)
	if err != nil {
		return nil, err
	}
	grouped := make(map[string][]CartItemResponse)
	shopNames := make(map[string]string)
	for combinationId, it := range items {
		grouped[it.ShopId] = append(grouped[it.ShopId], toCartItemResponse(combinationId, it))
		shopNames[it.ShopId] = it.ShopName
	}
	shopIds := sortedKeys(grouped)
	result := make([]ShopCartGroup, 0, len(shopIds))
	for _, shopId := range shopIds {
		result = append(result, ShopCartGroup{ShopId: shopId, ShopName: shopNames[shopId], Items: grouped[shopId]})
	}
	return result, nil
}

func (uc *CartUseCase) GetSelectedSummary(ctx context.Context, userId string) ([]ShopCheckoutInfo, error) {
	items, err := uc.repo.GetAll(ctx, userId)
	if err != nil {
		return nil, err
	}
	grouped := make(map[string][]CheckoutItem)
	for combinationId, it := range items {
		if !it.IsSelected {
			continue
		}
		grouped[it.ShopId] = append(grouped[it.ShopId], CheckoutItem{
			ShopId:        it.ShopId,
			CombinationId: combinationId,
			Quantity:      it.Quantity,
			ProductId:     it.ProductId,
			ProductName:   it.ProductName,
			ThumbnailUrl:  it.ThumbnailUrl,
			Variation:     it.Variation,
		})
	}
	shopIds := sortedKeysCheckout(grouped)
	result := make([]ShopCheckoutInfo, 0, len(shopIds))
	for _, shopId := range shopIds {
		result = append(result, ShopCheckoutInfo{ShopId: shopId, Items: grouped[shopId]})
	}
	return result, nil
}

func (uc *CartUseCase) ClearCart(ctx context.Context, userId string) error {
	return uc.repo.Clear(ctx, userId)
}

func toCartItemResponse(combinationId string, it domain.CartItem) CartItemResponse {
	return CartItemResponse{
		CombinationId: combinationId,
		ProductId:     it.ProductId,
		ShopId:        it.ShopId,
		ProductName:   it.ProductName,
		ThumbnailUrl:  it.ThumbnailUrl,
		Variation:     it.Variation,
		Quantity:      it.Quantity,
		IsSelected:    it.IsSelected,
		PriceSnapshot: it.PriceSnapshot,
		AddedAt:       it.AddedAt,
	}
}

func sortedKeys(m map[string][]CartItemResponse) []string {
	keys := make([]string, 0, len(m))
	for k := range m {
		keys = append(keys, k)
	}
	sort.Strings(keys)
	return keys
}

func sortedKeysCheckout(m map[string][]CheckoutItem) []string {
	keys := make([]string, 0, len(m))
	for k := range m {
		keys = append(keys, k)
	}
	sort.Strings(keys)
	return keys
}
