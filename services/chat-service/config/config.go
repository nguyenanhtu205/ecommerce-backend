package config

import (
	"os"

	"github.com/joho/godotenv"
)

type Config struct {
	MongoURI                       string
	MongoDBName                    string
	KafkaBrokers                   string
	KafkaTopicMessageSent          string
	KafkaTopicMediaAttached        string
	KafkaTopicShopChatSettings     string
	KafkaTopicShopVacationSettings string
	KafkaConsumerGroupID           string
	Port                           string
}

func Load() Config {
	_ = godotenv.Load()

	return Config{
		MongoURI:                       getEnv("MONGO_URI", "mongodb://localhost:27017"),
		MongoDBName:                    getEnv("MONGO_DB_NAME", "chat_db"),
		KafkaBrokers:                   getEnv("KAFKA_BROKERS", "localhost:9092"),
		KafkaTopicMessageSent:          getEnv("KAFKA_TOPIC_MESSAGE_SENT", "chat.message-sent.v1"),
		KafkaTopicMediaAttached:        getEnv("KAFKA_TOPIC_MEDIA_ATTACHED", "chat.message-media-attached.v1"),
		KafkaTopicShopChatSettings:     getEnv("KAFKA_TOPIC_SHOP_CHAT_SETTINGS", "seller.shop-chat-settings-updated.v1"),
		KafkaTopicShopVacationSettings: getEnv("KAFKA_TOPIC_SHOP_VACATION_SETTINGS", "seller.shop-vacation-settings-updated.v1"),
		KafkaConsumerGroupID:           getEnv("KAFKA_CONSUMER_GROUP_ID", "chat-service-group"),
		Port:                           getEnv("PORT", "8080"),
	}
}

func getEnv(key, fallback string) string {
	if v := os.Getenv(key); v != "" {
		return v
	}
	return fallback
}
