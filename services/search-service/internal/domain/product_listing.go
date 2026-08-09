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
	Location         string             `json:"location"`
	CategoryPath     []CategoryPathItem `json:"categoryPath"`
	PriceMin         int64              `json:"priceMin,string"`
	PriceMax         int64              `json:"priceMax,string"`
	OriginalPriceMin *int64             `json:"originalPriceMin,string"`
	DiscountPercent  *int               `json:"discountPercent"`
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
