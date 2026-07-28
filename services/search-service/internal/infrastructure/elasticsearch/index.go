package elasticsearch

import (
	"bytes"
	"context"
	"fmt"
	"io"

	es "github.com/elastic/go-elasticsearch/v8"
	"github.com/elastic/go-elasticsearch/v8/esapi"
)

const indexMapping = `{
  "settings": {
    "analysis": {
      "analyzer": {
        "edge_ngram_analyzer": { "type": "custom", "tokenizer": "edge_ngram_tokenizer" }
      },
      "tokenizer": {
        "edge_ngram_tokenizer": {
          "type": "edge_ngram", "min_gram": 1, "max_gram": 20,
          "token_chars": ["letter", "digit"]
        }
      }
    }
  },
  "mappings": {
    "properties": {
      "shopId": { "type": "keyword" },
      "shopName": { "type": "text", "fields": { "raw": { "type": "keyword" } } },
      "name": {
        "type": "text",
        "fields": {
          "raw": { "type": "keyword" },
          "suggest": { "type": "text", "analyzer": "edge_ngram_analyzer", "search_analyzer": "standard" }
        }
      },
      "description": { "type": "text" },
      "brand": { "type": "text", "fields": { "raw": { "type": "keyword" } } },
      "tags": { "type": "text", "fields": { "raw": { "type": "keyword" } } },
      "searchableSpecs": { "type": "text" },
      "thumbnailUrl": { "type": "keyword" },
      "categoryPath": {
        "type": "nested",
        "properties": {
          "id": { "type": "keyword" },
          "name": { "type": "keyword" }
        }
      },
      "priceMin": { "type": "integer" },
      "priceMax": { "type": "integer" },
      "originalPriceMin": { "type": "integer" },
      "discountPercent": { "type": "integer" },
      "stockTotal": { "type": "integer" },
      "isOutOfStock": { "type": "boolean" },
      "ratingAverage": { "type": "float" },
      "ratingCount": { "type": "integer" },
      "soldCount": { "type": "integer" }
    }
  }
}`

func EnsureIndex(ctx context.Context, client *es.Client, indexName string) error {
	existsRes, err := client.Indices.Exists(
		[]string{indexName},
		client.Indices.Exists.WithContext(ctx),
	)
	if err != nil {
		return fmt.Errorf("check index exist: %w", err)
	}
	defer func(Body io.ReadCloser) {
		err := Body.Close()
		if err != nil {
		}
	}(existsRes.Body)

	if existsRes.StatusCode == 200 {
		return nil
	}

	createRes, err := client.Indices.Create(
		indexName,
		client.Indices.Create.WithContext(ctx),
		client.Indices.Create.WithBody(bytes.NewReader([]byte(indexMapping))),
	)
	if err != nil {
		return fmt.Errorf("create index: %w", err)
	}
	defer func(Body io.ReadCloser) {
		err := Body.Close()
		if err != nil {
		}
	}(createRes.Body)

	if createRes.IsError() {
		b, _ := io.ReadAll(createRes.Body)
		return fmt.Errorf("elasticsearch create index error [%s]: %s", createRes.Status(), string(b))
	}

	return nil
}

var _ = esapi.IndicesExistsRequest{}
