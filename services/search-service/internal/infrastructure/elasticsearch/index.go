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
        "vi_folding_analyzer": {
          "type": "custom",
          "tokenizer": "standard",
          "filter": ["lowercase", "asciifolding"]
        },
        "vi_synonym_search_analyzer": {
          "type": "custom",
          "tokenizer": "standard",
          "filter": ["lowercase", "asciifolding"]
        },
        "edge_ngram_analyzer": { "type": "custom", "tokenizer": "edge_ngram_tokenizer", "filter": ["lowercase"] }
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
      "shopName": {
        "type": "text",
        "fields": {
          "raw": { "type": "keyword" },
          "folded": {
            "type": "text",
            "analyzer": "vi_folding_analyzer",
            "search_analyzer": "vi_synonym_search_analyzer"
          }
        }
      },
      "name": {
        "type": "text",
        "fields": {
          "raw": { "type": "keyword" },
          "folded": {
            "type": "text",
            "analyzer": "vi_folding_analyzer",
            "search_analyzer": "vi_synonym_search_analyzer"
          },
          "en": { "type": "text", "analyzer": "english" },
          "suggest": { "type": "text", "analyzer": "edge_ngram_analyzer", "search_analyzer": "standard" }
        }
      },
      "description": {
        "type": "text",
        "fields": {
          "folded": {
            "type": "text",
            "analyzer": "vi_folding_analyzer",
            "search_analyzer": "vi_synonym_search_analyzer"
          },
          "en": { "type": "text", "analyzer": "english" }
        }
      },
      "brand": {
        "type": "text",
        "fields": {
          "raw": { "type": "keyword" },
          "folded": {
            "type": "text",
            "analyzer": "vi_folding_analyzer",
            "search_analyzer": "vi_synonym_search_analyzer"
          }
        }
      },
      "tags": {
        "type": "text",
        "fields": {
          "raw": { "type": "keyword" },
          "folded": {
            "type": "text",
            "analyzer": "vi_folding_analyzer",
            "search_analyzer": "vi_synonym_search_analyzer"
          }
        }
      },
      "searchableSpecs": {
        "type": "text",
        "fields": {
          "folded": {
            "type": "text",
            "analyzer": "vi_folding_analyzer",
            "search_analyzer": "vi_synonym_search_analyzer"
          }
        }
      },
      "thumbnailUrl": { "type": "keyword" },
      "categoryPath": {
        "type": "nested",
        "properties": {
          "id": { "type": "keyword" },
          "name": { "type": "keyword", "copy_to": "categoryText" }
        }
      },
      "categoryText": {
        "type": "text",
        "fields": {
          "folded": {
            "type": "text",
            "analyzer": "vi_folding_analyzer",
            "search_analyzer": "vi_synonym_search_analyzer"
          }
        }
      },
      "priceMin": { "type": "long" },
      "priceMax": { "type": "long" },
      "originalPriceMin": { "type": "long" },
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
