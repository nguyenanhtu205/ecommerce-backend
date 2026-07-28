package application

import "context"

type TrendingKeyword struct {
	Keyword string
	Score   float64
}

type TrendingRepository interface {
	RecordSearch(ctx context.Context, keyword string) error

	TopKeywords(ctx context.Context, limit int) ([]TrendingKeyword, error)
}
