// @title Media Service API
// @version 1.0
// @BasePath /media
package main

import (
	"context"
	"database/sql"
	"errors"
	"log"
	kafkainfra "media-service/internal/infrastructure/kafka"
	kongauth "media-service/internal/interfaces/http/middleware"
	"net/http"
	"os/signal"
	"syscall"
	"time"

	_ "github.com/jackc/pgx/v5/stdlib"
	"github.com/joho/godotenv"
	"github.com/labstack/echo/v4"
	"github.com/labstack/echo/v4/middleware"
	echoSwagger "github.com/swaggo/echo-swagger"

	"media-service/config"
	_ "media-service/docs"
	"media-service/internal/application"
	"media-service/internal/infrastructure/minio"
	"media-service/internal/infrastructure/postgres"
	httpapi "media-service/internal/interfaces/http"
)

const httpShutdownTimeout = 10 * time.Second

func main() {
	_ = godotenv.Load()

	cfg := config.Load()

	db, err := sql.Open("pgx", cfg.DatabaseURL)
	if err != nil {
		log.Fatalf("connect postgres: %v", err)
	}
	defer func(db *sql.DB) {
		if err := db.Close(); err != nil {
			log.Printf("close postgres: %v", err)
		}
	}(db)
	if err := db.Ping(); err != nil {
		log.Fatalf("ping postgres: %v", err)
	}

	storage, err := minio.NewObjectStorage(
		cfg.MinioEndpoint,       
		cfg.MinioPublicEndpoint,
		cfg.MinioAccessKey,
		cfg.MinioSecretKey,
		cfg.MinioUseSSL,
		cfg.MinioPublicUseSSL,
	)
	if err != nil {
		log.Fatalf("init minio: %v", err)
	}

	events := kafkainfra.NewPublisher(cfg.KafkaBrokers, cfg.KafkaTopic)
	defer func(events *kafkainfra.Publisher) {
		if err := events.Close(); err != nil {
			log.Printf("close kafka publisher: %v", err)
		}
	}(events)

	repo := postgres.NewMediaRepository(db)
	svc := application.NewMediaService(repo, storage, events, cfg.MinioBucket)
	handler := httpapi.NewHandler(svc)

	ctx, stop := signal.NotifyContext(context.Background(), syscall.SIGINT, syscall.SIGTERM)
	defer stop()

	consumer := kafkainfra.NewConsumer(
		cfg.KafkaBrokers, cfg.ProductMediaAttachedTopic, cfg.ProductMediaAttachedGroupID, svc)
	go func() {
		if err := consumer.Start(ctx); err != nil {
			log.Printf("kafka consumer stopped with error: %v", err)
		}
	}()

	e := echo.New()
	e.GET("/", func(c echo.Context) error {
		return c.Redirect(http.StatusMovedPermanently, "/swagger/index.html")
	})
	e.GET("/swagger/*", echoSwagger.WrapHandler)
	e.Use(middleware.RequestLogger())
	e.Use(middleware.Recover())

	httpapi.RegisterRoutes(e, handler, kongauth.KongAuth())

	go func() {
		log.Printf("media-service listening on :%s", cfg.Port)
		if err := e.Start(":" + cfg.Port); err != nil && !errors.Is(err, http.ErrServerClosed) {
			log.Fatalf("server stopped with error: %v", err)
		}
	}()

	<-ctx.Done()
	log.Println("received shutdown signal, cleaning up...")

	if err := consumer.Close(); err != nil {
		log.Printf("close kafka consumer error: %v", err)
	}

	shutdownCtx, cancel := context.WithTimeout(context.Background(), httpShutdownTimeout)
	defer cancel()
	if err := e.Shutdown(shutdownCtx); err != nil {
		log.Printf("shutdown HTTP server error: %v", err)
	}
}
