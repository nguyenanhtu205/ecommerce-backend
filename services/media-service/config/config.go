package config

import (
	"os"
	"strings"
)

type Config struct {
	Port                        string
	DatabaseURL                 string
	MinioEndpoint               string
	MinioPublicEndpoint         string
	MinioAccessKey              string
	MinioSecretKey              string
	MinioBucket                 string
	MinioUseSSL                 bool
	MinioPublicUseSSL           bool
	KafkaBrokers                []string
	KafkaTopic                  string
	ProductMediaAttachedTopic   string
	ProductMediaAttachedGroupID string
}

func Load() Config {
	return Config{
		Port:                        getEnv("PORT", "8080"),
		DatabaseURL:                 os.Getenv("DATABASE_URL"),
		MinioEndpoint:               os.Getenv("MINIO_ENDPOINT"),
		MinioPublicEndpoint:         os.Getenv("MINIO_PUBLIC_ENDPOINT"),
		MinioAccessKey:              os.Getenv("MINIO_ACCESS_KEY"),
		MinioSecretKey:              os.Getenv("MINIO_SECRET_KEY"),
		MinioBucket:                 os.Getenv("MINIO_BUCKET"),
		MinioUseSSL:                 os.Getenv("MINIO_USE_SSL") == "true",
		MinioPublicUseSSL:           os.Getenv("MINIO_PUBLIC_USE_SSL") == "true",
		KafkaBrokers:                splitAndTrim(os.Getenv("KAFKA_BROKERS")),
		KafkaTopic:                  getEnv("KAFKA_TOPIC", "media-events"),
		ProductMediaAttachedTopic:   os.Getenv("KAFKA_PRODUCT_MEDIA_ATTACHED_TOPIC"),
		ProductMediaAttachedGroupID: os.Getenv("KAFKA_PRODUCT_MEDIA_ATTACHED_GROUP_ID"),
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
