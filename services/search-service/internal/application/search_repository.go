package application

import (
	"context"

	"search-service/internal/domain"
)

type SortField string

const (
	SortByPrice     SortField = "price"
	SortByRating    SortField = "rating"
	SortBySoldCount SortField = "soldCount"
)

type SortOrder string

const (
	SortAsc  SortOrder = "asc"
	SortDesc SortOrder = "desc"
)

type SearchFilters struct {
	PriceMin *int64
	PriceMax *int64
	Category string
}

type SortOption struct {
	Field SortField
	Order SortOrder
}

type Page struct {
	Number int
	Size   int
}

func (p Page) From() int {
	if p.Number <= 1 {
		return 0
	}
	return (p.Number - 1) * p.Size
}

type SearchResult struct {
	Total int64
	Items []domain.ProductListing
}

type SearchRepository interface {
	Upsert(ctx context.Context, doc domain.ProductListing) error

	Search(ctx context.Context, query string, filters SearchFilters, sort *SortOption, page Page) (*SearchResult, error)

	Suggest(ctx context.Context, prefix string, limit int) ([]string, error)
}
