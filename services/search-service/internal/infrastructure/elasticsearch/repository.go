package elasticsearch

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"strings"

	es "github.com/elastic/go-elasticsearch/v8"
	"github.com/elastic/go-elasticsearch/v8/esapi"

	"search-service/internal/application"
	"search-service/internal/domain"
)

type ESSearchRepository struct {
	client *es.Client
	index  string
}

func NewESSearchRepository(client *es.Client, index string) *ESSearchRepository {
	return &ESSearchRepository{client: client, index: index}
}

func (r *ESSearchRepository) Upsert(ctx context.Context, doc domain.ProductListing) error {
	body, err := json.Marshal(doc)
	if err != nil {
		return fmt.Errorf("marshal document: %w", err)
	}

	version, err := doc.SyncedAtUnixNano()
	if err != nil {
		return fmt.Errorf("compute version: %w", err)
	}
	v := int(version)

	req := esapi.IndexRequest{
		Index:       r.index,
		DocumentID:  doc.DocumentID(),
		Body:        bytes.NewReader(body),
		Refresh:     "false",
		VersionType: "external",
		Version:     &v,
	}

	res, err := req.Do(ctx, r.client)
	if err != nil {
		return fmt.Errorf("index request: %w", err)
	}
	defer func(Body io.ReadCloser) {
		err := Body.Close()
		if err != nil {
		}
	}(res.Body)

	if res.StatusCode == http.StatusConflict {
		return nil
	}

	if res.IsError() {
		b, _ := io.ReadAll(res.Body)
		return fmt.Errorf("elasticsearch index error [%s]: %s", res.Status(), string(b))
	}

	return nil
}

func (r *ESSearchRepository) Search(ctx context.Context, query string, filters application.SearchFilters, sort *application.SortOption, page application.Page) (*application.SearchResult, error) {
	var must []map[string]any
	if query != "" {
		foldedFields := []string{
			"name.folded^3", "description.folded", "brand.folded^2",
			"tags.folded", "shopName.folded", "searchableSpecs.folded", "categoryText.folded", "location.folded",
		}

		must = append(must, map[string]any{
			"bool": map[string]any{
				"should": []map[string]any{
					{
						"multi_match": map[string]any{
							"query":     query,
							"fields":    foldedFields,
							"type":      "best_fields",
							"fuzziness": "AUTO",
						},
					},
					{
						"multi_match": map[string]any{
							"query":  query,
							"fields": foldedFields,
							"type":   "phrase",
							"boost":  3,
						},
					},
					{
						"multi_match": map[string]any{
							"query":  query,
							"fields": []string{"name.en^2", "description.en"},
							"type":   "best_fields",
							"boost":  0.5,
						},
					},
				},
				"minimum_should_match": 1,
			},
		})
	} else {
		must = append(must, map[string]any{"match_all": map[string]any{}})
	}

	var filter []map[string]any
	if filters.PriceMin != nil || filters.PriceMax != nil {
		rangeClause := map[string]any{}
		if filters.PriceMin != nil {
			rangeClause["gte"] = *filters.PriceMin
		}
		if filters.PriceMax != nil {
			rangeClause["lte"] = *filters.PriceMax
		}
		filter = append(filter, map[string]any{
			"range": map[string]any{"priceMin": rangeClause},
		})
	}
	if filters.Category != "" {
		filter = append(filter, map[string]any{
			"nested": map[string]any{
				"path": "categoryPath",
				"query": map[string]any{
					"term": map[string]any{"categoryPath.id": filters.Category},
				},
			},
		})
	}
	if filters.Location != "" {
		filter = append(filter, map[string]any{
			"term": map[string]any{"location": filters.Location},
		})
	}

	queryBody := map[string]any{
		"from": page.From(),
		"size": page.Size,
		"query": map[string]any{
			"bool": map[string]any{
				"must":   must,
				"filter": filter,
			},
		},
	}

	if sort != nil {
		esField := mapSortField(sort.Field)
		queryBody["sort"] = []map[string]any{
			{esField: map[string]any{"order": string(sort.Order)}},
		}
	}

	buf, err := json.Marshal(queryBody)
	if err != nil {
		return nil, fmt.Errorf("marshal search query: %w", err)
	}

	res, err := r.client.Search(
		r.client.Search.WithContext(ctx),
		r.client.Search.WithIndex(r.index),
		r.client.Search.WithBody(bytes.NewReader(buf)),
	)
	if err != nil {
		return nil, fmt.Errorf("search request: %w", err)
	}
	defer func(Body io.ReadCloser) {
		err := Body.Close()
		if err != nil {
		}
	}(res.Body)

	if res.IsError() {
		b, _ := io.ReadAll(res.Body)
		return nil, fmt.Errorf("elasticsearch search error [%s]: %s", res.Status(), string(b))
	}

	var parsed esSearchResponse
	if err := json.NewDecoder(res.Body).Decode(&parsed); err != nil {
		return nil, fmt.Errorf("decode search response: %w", err)
	}

	items := make([]domain.ProductListing, 0, len(parsed.Hits.Hits))
	for _, h := range parsed.Hits.Hits {
		items = append(items, h.Source)
	}

	return &application.SearchResult{
		Total: parsed.Hits.Total.Value,
		Items: items,
	}, nil
}

func (r *ESSearchRepository) Suggest(ctx context.Context, prefix string, limit int) ([]string, error) {
	prefix = strings.ToLower(strings.TrimSpace(prefix))

	queryBody := map[string]any{
		"size":    limit,
		"_source": []string{"name"},
		"query": map[string]any{
			"prefix": map[string]any{
				"name.suggest": map[string]any{
					"value": prefix,
				},
			},
		},
	}

	buf, err := json.Marshal(queryBody)
	if err != nil {
		return nil, fmt.Errorf("marshal suggest query: %w", err)
	}

	res, err := r.client.Search(
		r.client.Search.WithContext(ctx),
		r.client.Search.WithIndex(r.index),
		r.client.Search.WithBody(bytes.NewReader(buf)),
	)
	if err != nil {
		return nil, fmt.Errorf("suggest request: %w", err)
	}
	defer func(Body io.ReadCloser) {
		err := Body.Close()
		if err != nil {
		}
	}(res.Body)

	if res.IsError() {
		b, _ := io.ReadAll(res.Body)
		return nil, fmt.Errorf("elasticsearch suggest error [%s]: %s", res.Status(), string(b))
	}

	var parsed esSearchResponse
	if err := json.NewDecoder(res.Body).Decode(&parsed); err != nil {
		return nil, fmt.Errorf("decode suggest response: %w", err)
	}

	seen := make(map[string]struct{}, len(parsed.Hits.Hits))
	suggestions := make([]string, 0, len(parsed.Hits.Hits))
	for _, h := range parsed.Hits.Hits {
		if _, ok := seen[h.Source.Name]; ok {
			continue
		}
		seen[h.Source.Name] = struct{}{}
		suggestions = append(suggestions, h.Source.Name)
	}

	return suggestions, nil
}

func mapSortField(f application.SortField) string {
	switch f {
	case application.SortByPrice:
		return "priceMin"
	case application.SortByRating:
		return "ratingAverage"
	case application.SortBySoldCount:
		return "soldCount"
	default:
		return "_score"
	}
}

type esSearchResponse struct {
	Hits struct {
		Total struct {
			Value int64 `json:"value"`
		} `json:"total"`
		Hits []struct {
			Source domain.ProductListing `json:"_source"`
		} `json:"hits"`
	} `json:"hits"`
}
