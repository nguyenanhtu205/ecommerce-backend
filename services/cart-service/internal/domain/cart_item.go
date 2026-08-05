package domain

type CartItem struct {
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
