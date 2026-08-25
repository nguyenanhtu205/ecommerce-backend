package redis

import (
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"time"

	"github.com/redis/go-redis/v9"

	"search-service/internal/application"
)

type SearchCacheRepository struct {
	client *redis.Client
}

func NewRedisSearchCacheRepository(client *redis.Client) *SearchCacheRepository {
	return &SearchCacheRepository{client: client}
}

func (r *SearchCacheRepository) Get(ctx context.Context, keyword string) (*application.SearchResult, error) {
	val, err := r.client.Get(ctx, cacheKey(keyword)).Result()
	if err != nil {
		if errors.Is(err, redis.Nil) {
			return nil, nil
		}
		return nil, fmt.Errorf("get search cache: %w", err)
	}

	var result application.SearchResult
	if err := json.Unmarshal([]byte(val), &result); err != nil {
		return nil, fmt.Errorf("unmarshal search cache: %w", err)
	}

	return &result, nil
}

func (r *SearchCacheRepository) Set(ctx context.Context, keyword string, result *application.SearchResult, ttl time.Duration) error {
	buf, err := json.Marshal(result)
	if err != nil {
		return fmt.Errorf("marshal search cache: %w", err)
	}

	if err := r.client.Set(ctx, cacheKey(keyword), buf, ttl).Err(); err != nil {
		return fmt.Errorf("set search cache: %w", err)
	}

	return nil
}

func cacheKey(keyword string) string {
	return fmt.Sprintf("search:cache:%s", keyword)
}
