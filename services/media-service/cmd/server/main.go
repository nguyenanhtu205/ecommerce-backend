// @title Media Service API
// @version 1.0
// @description Media Service của Shopee clone
// @BasePath /media
package main

import (
	"database/sql"
	"log"
	kafkainfra "media-service/internal/infrastructure/kafka"

	_ "github.com/jackc/pgx/v5/stdlib"
	"github.com/joho/godotenv"
	"github.com/labstack/echo/v4"
	"github.com/labstack/echo/v4/middleware"
	echoSwagger "github.com/swaggo/echo-swagger"

	"media-service/config"
	"media-service/internal/application"
	"media-service/internal/infrastructure/minio"
	"media-service/internal/infrastructure/postgres"
	httpapi "media-service/internal/interfaces/http"
	//kongauth "media-service/internal/interfaces/http/middleware"

	_ "media-service/docs"
)

func main() {
	_ = godotenv.Load()

	cfg := config.Load()

	db, err := sql.Open("pgx", cfg.DatabaseURL)
	if err != nil {
		log.Fatalf("connect postgres: %v", err)
	}
	defer func(db *sql.DB) {
		err := db.Close()
		if err != nil {
			log.Fatalf("close postgres: %v", err)
		}
	}(db)
	if err := db.Ping(); err != nil {
		log.Fatalf("ping postgres: %v", err)
	}

	storage, err := minio.NewObjectStorage(cfg.MinioEndpoint, cfg.MinioAccessKey, cfg.MinioSecretKey, cfg.MinioUseSSL)
	if err != nil {
		log.Fatalf("init minio: %v", err)
	}

	events := kafkainfra.NewPublisher(cfg.KafkaBrokers, cfg.KafkaTopic)
	defer func(events *kafkainfra.Publisher) {
		err := events.Close()
		if err != nil {
			log.Fatalf("close kafka publisher: %v", err)
		}
	}(events)

	repo := postgres.NewMediaRepository(db)
	svc := application.NewMediaService(repo, storage, events, cfg.MinioBucket)
	handler := httpapi.NewHandler(svc)

	e := echo.New()
	e.GET("/swagger/*", echoSwagger.WrapHandler)
	e.Use(middleware.RequestLogger())
	e.Use(middleware.Recover())

	//e.Use(kongauth.KongAuth())

	httpapi.RegisterRoutes(e, handler)

	log.Printf("media-service listening on :%s", cfg.Port)
	if err := e.Start(":" + cfg.Port); err != nil {
		log.Fatalf("server error: %v", err)
	}
}
