package domain

type CartItem struct {
	ProductId     string       `json:"productId"`
	ShopId        string       `json:"shopId"`
	ShopName      string       `json:"shopName"`
	ProductName   string       `json:"productName"`
	ThumbnailUrl  string       `json:"thumbnailUrl"`
	Variation     *string      `json:"variation"`
	Quantity      int          `json:"quantity"`
	IsSelected    bool         `json:"isSelected"`
	PriceSnapshot int          `json:"priceSnapshot"`
	AddedAt       string       `json:"addedAt"`
	ShippingInfo  ShippingInfo `json:"shippingInfo"`
}

type ShippingInfo struct {
	WeightGrams float64    `json:"weightGrams"`
	Dimensions  Dimensions `json:"dimensions"`
}

type Dimensions struct {
	Length float64 `json:"length"`
	Width  float64 `json:"width"`
	Height float64 `json:"height"`
}
