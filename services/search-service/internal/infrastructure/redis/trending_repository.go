package redis

import (
	"context"
	"fmt"
	"time"

	"github.com/redis/go-redis/v9"

	"search-service/internal/application"
)

const trendingTTL = 48 * time.Hour

type TrendingRepository struct {
	client *redis.Client
}

func NewRedisTrendingRepository(client *redis.Client) *TrendingRepository {
	return &TrendingRepository{client: client}
}

func (r *TrendingRepository) RecordSearch(ctx context.Context, keyword string) error {
	key := dailyKey(time.Now())

	if err := r.client.ZIncrBy(ctx, key, 1, keyword).Err(); err != nil {
		return fmt.Errorf("zincrby trending: %w", err)
	}

	if err := r.client.Expire(ctx, key, trendingTTL).Err(); err != nil {
		return fmt.Errorf("expire trending key: %w", err)
	}

	return nil
}

func (r *TrendingRepository) TopKeywords(ctx context.Context, limit int) ([]application.TrendingKeyword, error) {
	key := dailyKey(time.Now())

	results, err := r.client.ZRevRangeWithScores(ctx, key, 0, int64(limit-1)).Result()
	if err != nil {
		return nil, fmt.Errorf("zrevrange trending: %w", err)
	}

	keywords := make([]application.TrendingKeyword, 0, len(results))
	for _, z := range results {
		keyword, ok := z.Member.(string)
		if !ok {
			continue
		}
		keywords = append(keywords, application.TrendingKeyword{
			Keyword: keyword,
			Score:   z.Score,
		})
	}

	return keywords, nil
}

func dailyKey(t time.Time) string {
	return fmt.Sprintf("trending:keywords:%s", t.UTC().Format("2006-01-02"))
}
