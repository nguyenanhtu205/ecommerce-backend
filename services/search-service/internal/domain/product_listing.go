package domain

type CategoryPathItem struct {
	ID   string `json:"id"`
	Name string `json:"name"`
}

type ProductListing struct {
	ProductID        string             `json:"productId"`
	ShopID           string             `json:"shopId"`
	ShopName         string             `json:"shopName"`
	Name             string             `json:"name"`
	Description      string             `json:"description"`
	Brand            *string            `json:"brand"`
	Tags             []string           `json:"tags"`
	SearchableSpecs  string             `json:"searchableSpecs"`
	ThumbnailURL     string             `json:"thumbnailUrl"`
	CategoryPath     []CategoryPathItem `json:"categoryPath"`
	PriceMin         int                `json:"priceMin"`
	PriceMax         int                `json:"priceMax"`
	OriginalPriceMin int                `json:"originalPriceMin"`
	DiscountPercent  int                `json:"discountPercent"`
	StockTotal       int                `json:"stockTotal"`
	IsOutOfStock     bool               `json:"isOutOfStock"`
	RatingAverage    float64            `json:"ratingAverage"`
	RatingCount      int                `json:"ratingCount"`
	SoldCount        int                `json:"soldCount"`
	SyncedAt         string             `json:"syncedAt"`
}

func (p ProductListing) DocumentID() string {
	return p.ProductID + "-" + p.SyncedAt
}
