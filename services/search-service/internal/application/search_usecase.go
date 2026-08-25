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
	searchCacheTTL   = 60 * time.Second
)

type SearchUseCase struct {
	repo         SearchRepository
	trendingRepo TrendingRepository
	cacheRepo    SearchCacheRepository
	trendingKeys *TrendingKeywordSet
}

func NewSearchUseCase(
	repo SearchRepository,
	trendingRepo TrendingRepository,
	cacheRepo SearchCacheRepository,
	trendingKeys *TrendingKeywordSet,
) *SearchUseCase {
	return &SearchUseCase{
		repo:         repo,
		trendingRepo: trendingRepo,
		cacheRepo:    cacheRepo,
		trendingKeys: trendingKeys,
	}
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

	cacheable := uc.cacheRepo != nil && uc.trendingKeys != nil &&
		query != "" && isPlainQuery(filters, sort, page) && uc.trendingKeys.Contains(query)

	if cacheable {
		if cached, err := uc.cacheRepo.Get(ctx, strings.ToLower(query)); err == nil && cached != nil {
			return cached, nil
		}
	}

	result, err := uc.repo.Search(ctx, query, filters, sort, page)
	if err != nil {
		return nil, err
	}

	if query != "" && uc.trendingRepo != nil {
		go uc.recordTrending(strings.ToLower(query))
	}

	if cacheable {
		go uc.cacheResult(strings.ToLower(query), result)
	}

	return result, nil
}

func isPlainQuery(filters SearchFilters, sort *SortOption, page Page) bool {
	return filters.PriceMin == nil &&
		filters.PriceMax == nil &&
		filters.Category == "" &&
		filters.Location == "" &&
		sort == nil &&
		page.Number == 1 &&
		page.Size == defaultPageSize
}

func (uc *SearchUseCase) cacheResult(keyword string, result *SearchResult) {
	ctx, cancel := context.WithTimeout(context.Background(), 2*time.Second)
	defer cancel()
	_ = uc.cacheRepo.Set(ctx, keyword, result, searchCacheTTL)
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
