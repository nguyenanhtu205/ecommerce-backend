package application

import (
	"context"
	"strings"
	"time"
)

const (
	defaultPageSize  = 20
	maxPageSize      = 100
	defaultSuggestN  = 10
	defaultTrendingN = 10
)

type SearchUseCase struct {
	repo         SearchRepository
	trendingRepo TrendingRepository
}

func NewSearchUseCase(repo SearchRepository, trendingRepo TrendingRepository) *SearchUseCase {
	return &SearchUseCase{repo: repo, trendingRepo: trendingRepo}
}

func (uc *SearchUseCase) Search(ctx context.Context, query string, filters SearchFilters, sort *SortOption, page Page) (*SearchResult, error) {
	query = strings.TrimSpace(query)

	if page.Number < 1 {
		page.Number = 1
	}
	if page.Size < 1 {
		page.Size = defaultPageSize
	}
	if page.Size > maxPageSize {
		page.Size = maxPageSize
	}

	result, err := uc.repo.Search(ctx, query, filters, sort, page)
	if err != nil {
		return nil, err
	}

	if query != "" && uc.trendingRepo != nil {
		go uc.recordTrending(strings.ToLower(query))
	}

	return result, nil
}

func (uc *SearchUseCase) recordTrending(keyword string) {
	ctx, cancel := context.WithTimeout(context.Background(), 2*time.Second)
	defer cancel()
	_ = uc.trendingRepo.RecordSearch(ctx, keyword)
}

func (uc *SearchUseCase) Suggest(ctx context.Context, prefix string) ([]string, error) {
	prefix = strings.TrimSpace(prefix)
	if prefix == "" {
		return []string{}, nil
	}
	return uc.repo.Suggest(ctx, prefix, defaultSuggestN)
}

func (uc *SearchUseCase) GetTrending(ctx context.Context, limit int) ([]TrendingKeyword, error) {
	if uc.trendingRepo == nil {
		return []TrendingKeyword{}, nil
	}
	if limit < 1 {
		limit = defaultTrendingN
	}
	return uc.trendingRepo.TopKeywords(ctx, limit)
}
