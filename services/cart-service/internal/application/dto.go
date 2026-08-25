package application

import "cart-service/internal/domain"

type AddItemRequest struct {
	CombinationId string              `json:"combinationId"`
	ProductId     string              `json:"productId"`
	ShopId        string              `json:"shopId"`
	ShopName      string              `json:"shopName"`
	ProductName   string              `json:"productName"`
	ThumbnailUrl  string              `json:"thumbnailUrl"`
	Variation     *string             `json:"variation"`
	Quantity      int                 `json:"quantity"`
	IsSelected    bool                `json:"isSelected"`
	PriceSnapshot int                 `json:"priceSnapshot"`
	ShippingInfo  domain.ShippingInfo `json:"shippingInfo"`
}

type UpdateItemRequest struct {
	Quantity   *int  `json:"quantity"`
	IsSelected *bool `json:"isSelected"`
}

type RemoveItemsRequest struct {
	CombinationIds []string `json:"combinationIds"`
}

type CartItemResponse struct {
	CombinationId string  `json:"combinationId"`
	ProductId     string  `json:"productId"`
	ShopId        string  `json:"shopId"`
	ProductName   string  `json:"productName"`
	ThumbnailUrl  string  `json:"thumbnailUrl"`
	Variation     *string `json:"variation"`
	Quantity      int     `json:"quantity"`
	IsSelected    bool    `json:"isSelected"`
	PriceSnapshot int     `json:"priceSnapshot"`
	AddedAt       string  `json:"addedAt"`
}

type ShopCartGroup struct {
	ShopId   string             `json:"shopId"`
	ShopName string             `json:"shopName"`
	Items    []CartItemResponse `json:"items"`
}

type CheckoutItem struct {
	CombinationId string              `json:"combinationId"`
	Quantity      int                 `json:"quantity"`
	ProductId     string              `json:"productId"`
	ProductName   string              `json:"productName"`
	PriceSnapshot int                 `json:"priceSnapshot"`
	ThumbnailUrl  string              `json:"thumbnailUrl"`
	Variation     *string             `json:"variation"`
	ShippingInfo  domain.ShippingInfo `json:"shippingInfo"`
}

type ShopCheckoutInfo struct {
	ShopName string         `json:"shopName"`
	ShopId   string         `json:"shopId"`
	Items    []CheckoutItem `json:"items"`
}
