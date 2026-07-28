package http

import (
	"fmt"
	"strings"

	"search-service/internal/application"
	"search-service/internal/domain"
)

type SearchResponse struct {
	Total int64                   `json:"total"`
	Items []domain.ProductListing `json:"items"`
}

type SuggestResponse struct {
	Suggestions []string `json:"suggestions"`
}

type TrendingItem struct {
	Keyword string  `json:"keyword"`
	Score   float64 `json:"score"`
}

type TrendingResponse struct {
	Items []TrendingItem `json:"items"`
}

var allowedSortFields = map[string]application.SortField{
	"price":     application.SortByPrice,
	"rating":    application.SortByRating,
	"soldCount": application.SortBySoldCount,
}

func parseSort(raw string) (*application.SortOption, error) {
	if raw == "" {
		return nil, nil
	}

	parts := strings.SplitN(raw, ":", 2)
	field, ok := allowedSortFields[parts[0]]
	if !ok {
		return nil, fmt.Errorf("invalid sort field: %s", parts[0])
	}

	order := application.SortDesc
	if len(parts) == 2 {
		switch strings.ToLower(parts[1]) {
		case "asc":
			order = application.SortAsc
		case "desc":
			order = application.SortDesc
		default:
			return nil, fmt.Errorf("invalid sort order: %s", parts[1])
		}
	}

	return &application.SortOption{Field: field, Order: order}, nil
}
