package config

import (
	"os"

	"github.com/joho/godotenv"
)

type Config struct {
	MongoURI              string
	MongoDBName           string
	KafkaBrokers          string
	KafkaTopicMessageSent string
	Port                  string
}

func Load() Config {
	_ = godotenv.Load()

	return Config{
		MongoURI:              getEnv("MONGO_URI", "mongodb://localhost:27017"),
		MongoDBName:           getEnv("MONGO_DB_NAME", "chat_db"),
		KafkaBrokers:          getEnv("KAFKA_BROKERS", "localhost:9092"),
		KafkaTopicMessageSent: getEnv("KAFKA_TOPIC_MESSAGE_SENT", "chat.message-sent.v1"),
		Port:                  getEnv("PORT", "8080"),
	}
}

func getEnv(key, fallback string) string {
	if v := os.Getenv(key); v != "" {
		return v
	}
	return fallback
}
