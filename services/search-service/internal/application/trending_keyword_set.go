package application

import (
	"context"
	"strings"
	"sync"
	"time"
)

type TrendingKeywordSet struct {
	repo     TrendingRepository
	limit    int
	interval time.Duration

	mu   sync.RWMutex
	keys map[string]struct{}
}

func NewTrendingKeywordSet(repo TrendingRepository, limit int, interval time.Duration) *TrendingKeywordSet {
	return &TrendingKeywordSet{
		repo:     repo,
		limit:    limit,
		interval: interval,
		keys:     make(map[string]struct{}),
	}
}

func (s *TrendingKeywordSet) StartRefreshing(ctx context.Context) {
	s.refresh(ctx)

	ticker := time.NewTicker(s.interval)
	defer ticker.Stop()

	for {
		select {
		case <-ctx.Done():
			return
		case <-ticker.C:
			s.refresh(ctx)
		}
	}
}

func (s *TrendingKeywordSet) refresh(ctx context.Context) {
	top, err := s.repo.TopKeywords(ctx, s.limit)
	if err != nil {
		return
	}

	next := make(map[string]struct{}, len(top))
	for _, k := range top {
		next[strings.ToLower(k.Keyword)] = struct{}{}
	}

	s.mu.Lock()
	s.keys = next
	s.mu.Unlock()
}

func (s *TrendingKeywordSet) Contains(keyword string) bool {
	s.mu.RLock()
	defer s.mu.RUnlock()
	_, ok := s.keys[strings.ToLower(keyword)]
	return ok
}
