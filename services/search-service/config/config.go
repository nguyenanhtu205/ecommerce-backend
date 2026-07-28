package config

import (
	"os"
)

type Config struct {
	ESURL        string
	ESIndex      string
	KafkaBrokers string
	KafkaTopic   string
	KafkaGroupID string
	RedisAddr    string
	Port         string
}

func Load() Config {
	return Config{
		ESURL:        os.Getenv("ES_URL"),
		ESIndex:      os.Getenv("ES_INDEX"),
		KafkaBrokers: os.Getenv("KAFKA_BROKERS"),
		KafkaTopic:   os.Getenv("KAFKA_TOPIC"),
		KafkaGroupID: os.Getenv("KAFKA_GROUP_ID"),
		RedisAddr:    os.Getenv("REDIS_ADDR"),
		Port:         os.Getenv("PORT"),
	}
}
