// @title Chat Service API
// @version 1.0
package main

import (
	"context"
	"log"
	"net/http"
	"strings"

	"github.com/labstack/echo/v4"
	"github.com/labstack/echo/v4/middleware"
	echoSwagger "github.com/swaggo/echo-swagger"
	"go.mongodb.org/mongo-driver/v2/mongo"
	"go.mongodb.org/mongo-driver/v2/mongo/options"

	"chat-service/config"
	"chat-service/internal/application"
	kafkainfra "chat-service/internal/infrastructure/kafka"
	"chat-service/internal/infrastructure/mongodb"
	wsinfra "chat-service/internal/infrastructure/websocket"
	chathttp "chat-service/internal/interfaces/http"

	_ "chat-service/docs"
)

func main() {
	cfg := config.Load()
	ctx := context.Background()

	mongoClient, err := mongo.Connect(options.Client().ApplyURI(cfg.MongoURI))
	if err != nil {
		log.Fatalf("mongo connect error: %v", err)
	}
	defer func(mongoClient *mongo.Client, ctx context.Context) {
		err := mongoClient.Disconnect(ctx)
		if err != nil {

		}
	}(mongoClient, ctx)

	db := mongoClient.Database(cfg.MongoDBName)
	repo := mongodb.NewChatRepository(db)
	if err := repo.EnsureIndexes(ctx); err != nil {
		log.Fatalf("ensure indexes error: %v", err)
	}

	kafkaBrokers := strings.Split(cfg.KafkaBrokers, ",")
	publisher := kafkainfra.NewPublisher(kafkaBrokers, cfg.KafkaTopicMessageSent, cfg.KafkaTopicMediaAttached)
	defer func(publisher *kafkainfra.Publisher) {
		err := publisher.Close()
		if err != nil {
		}
	}(publisher)

	hub := wsinfra.NewHub()

	shopChatSettingsRepo := mongodb.NewShopChatSettingsRepository(db)
	ingestUseCase := application.NewShopChatSettingsIngestUseCase(shopChatSettingsRepo)

	chatSettingsConsumer := kafkainfra.NewConsumer(kafkaBrokers, cfg.KafkaTopicShopChatSettings, cfg.KafkaConsumerGroupID, ingestUseCase.HandleChatSettingsUpdated)
	vacationConsumer := kafkainfra.NewConsumer(kafkaBrokers, cfg.KafkaTopicShopVacationSettings, cfg.KafkaConsumerGroupID, ingestUseCase.HandleVacationSettingsUpdated)

	go func() {
		if err := chatSettingsConsumer.Start(ctx); err != nil {
			log.Printf("chat settings consumer stopped: %v", err)
		}
	}()
	go func() {
		if err := vacationConsumer.Start(ctx); err != nil {
			log.Printf("vacation settings consumer stopped: %v", err)
		}
	}()
	defer func(chatSettingsConsumer *kafkainfra.Consumer) {
		err := chatSettingsConsumer.Close()
		if err != nil {
		}
	}(chatSettingsConsumer)
	defer func(vacationConsumer *kafkainfra.Consumer) {
		err := vacationConsumer.Close()
		if err != nil {
		}
	}(vacationConsumer)

	uc := application.NewChatUseCase(repo, shopChatSettingsRepo, publisher, hub)

	handler := chathttp.NewChatHandler(uc)
	wsHandler := chathttp.NewChatWSHandler(hub, uc)

	e := echo.New()
	e.GET("/", func(c echo.Context) error {
		return c.Redirect(http.StatusMovedPermanently, "/swagger/index.html")
	})
	e.GET("/swagger/*", echoSwagger.WrapHandler)
	e.Use(middleware.RequestLogger())
	e.Use(middleware.Recover())
	chathttp.RegisterRoutes(e, handler, wsHandler)

	log.Fatal(e.Start(":" + cfg.Port))
}
