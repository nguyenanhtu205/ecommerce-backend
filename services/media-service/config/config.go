package config

import (
	"os"
	"strings"
)

type Config struct {
	Port                      string
	DatabaseURL               string
	MinioEndpoint             string
	MinioPublicEndpoint       string
	MinioAccessKey            string
	MinioSecretKey            string
	MinioBucket               string
	MinioUseSSL               bool
	MinioPublicUseSSL         bool
	KafkaBrokers              []string
	KafkaTopic                string
	ProductMediaAttachedTopic string
	ReviewMediaAttachedTopic  string
	MediaConsumerGroupID      string
}

func Load() Config {
	return Config{
		Port:                      getEnv("PORT", "8080"),
		DatabaseURL:               os.Getenv("DATABASE_URL"),
		MinioEndpoint:             os.Getenv("MINIO_ENDPOINT"),
		MinioPublicEndpoint:       getEnv("MINIO_PUBLIC_ENDPOINT", os.Getenv("MINIO_ENDPOINT")),
		MinioAccessKey:            os.Getenv("MINIO_ACCESS_KEY"),
		MinioSecretKey:            os.Getenv("MINIO_SECRET_KEY"),
		MinioBucket:               os.Getenv("MINIO_BUCKET"),
		MinioUseSSL:               os.Getenv("MINIO_USE_SSL") == "true",
		MinioPublicUseSSL:         os.Getenv("MINIO_PUBLIC_USE_SSL") == "true",
		KafkaBrokers:              splitAndTrim(os.Getenv("KAFKA_BROKERS")),
		KafkaTopic:                getEnv("KAFKA_TOPIC", "media-events"),
		ProductMediaAttachedTopic: getEnv("KAFKA_PRODUCT_MEDIA_ATTACHED_TOPIC", "product-catalog.product-media-attached.v1"),
		ReviewMediaAttachedTopic:  getEnv("KAFKA_REVIEW_MEDIA_ATTACHED_TOPIC", "review.review-media-attached.v1"),
		MediaConsumerGroupID:      getEnv("KAFKA_CONSUMER_GROUP_ID", "media-service-group"),
	}
}

func getEnv(key, fallback string) string {
	if v := os.Getenv(key); v != "" {
		return v
	}
	return fallback
}

func splitAndTrim(csv string) []string {
	if csv == "" {
		return nil
	}
	parts := strings.Split(csv, ",")
	out := make([]string, 0, len(parts))
	for _, p := range parts {
		if p = strings.TrimSpace(p); p != "" {
			out = append(out, p)
		}
	}
	return out
}
