package application

type AddItemRequest struct {
	CombinationId string  `json:"combinationId"`
	ProductId     string  `json:"productId"`
	ShopId        string  `json:"shopId"`
	ShopName      string  `json:"shopName"`
	ProductName   string  `json:"productName"`
	ThumbnailUrl  string  `json:"thumbnailUrl"`
	Variation     *string `json:"variation"`
	Quantity      int     `json:"quantity"`
	PriceSnapshot int     `json:"priceSnapshot"`
	IsSelected    bool    `json:"isSelected"`
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
	ShopId        string  `json:"shopId"`
	CombinationId string  `json:"combinationId"`
	Quantity      int     `json:"quantity"`
	ProductId     string  `json:"productId"`
	ProductName   string  `json:"productName"`
	ThumbnailUrl  string  `json:"thumbnailUrl"`
	Variation     *string `json:"variation"`
}

type ShopCheckoutInfo struct {
	ShopId string         `json:"shopId"`
	Items  []CheckoutItem `json:"items"`
}
