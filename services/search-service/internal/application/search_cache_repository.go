package application

import (
	"context"
	"time"
)

type SearchCacheRepository interface {
	Get(ctx context.Context, keyword string) (*SearchResult, error)

	Set(ctx context.Context, keyword string, result *SearchResult, ttl time.Duration) error
}
