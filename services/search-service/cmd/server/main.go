// @title Search Service API
// @version 1.0
// @BasePath /search
package main

import (
	"context"
	"errors"
	"log"
	"net/http"
	"os/signal"
	"strings"
	"syscall"

	_ "search-service/docs"

	es "github.com/elastic/go-elasticsearch/v8"
	"github.com/joho/godotenv"
	"github.com/labstack/echo/v4"
	"github.com/labstack/echo/v4/middleware"
	goredis "github.com/redis/go-redis/v9"
	echoSwagger "github.com/swaggo/echo-swagger"

	"search-service/config"
	"search-service/internal/application"
	"search-service/internal/infrastructure/elasticsearch"
	kafkainfra "search-service/internal/infrastructure/kafka"
	redisinfra "search-service/internal/infrastructure/redis"
	httpinterface "search-service/internal/interfaces/http"
)

func main() {
	_ = godotenv.Load()

	cfg := config.Load()

	esClient, err := es.NewClient(es.Config{Addresses: []string{cfg.ESURL}})
	if err != nil {
		log.Fatalf("init Elasticsearch client failed: %v", err)
	}

	if err := elasticsearch.EnsureIndex(context.Background(), esClient, cfg.ESIndex); err != nil {
		log.Fatalf("ensure index failed: %v", err)
	}

	repo := elasticsearch.NewESSearchRepository(esClient, cfg.ESIndex)

	var trendingRepo application.TrendingRepository
	if cfg.RedisAddr != "" {
		redisClient := goredis.NewClient(&goredis.Options{Addr: cfg.RedisAddr})
		trendingRepo = redisinfra.NewRedisTrendingRepository(redisClient)
	}

	ingestUseCase := application.NewIngestUseCase(repo)
	searchUseCase := application.NewSearchUseCase(repo, trendingRepo)

	ctx, stop := signal.NotifyContext(context.Background(), syscall.SIGINT, syscall.SIGTERM)
	defer stop()

	consumer := kafkainfra.NewConsumer(
		strings.Split(cfg.KafkaBrokers, ","),
		cfg.KafkaTopic,
		cfg.KafkaGroupID,
		ingestUseCase,
	)
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

	searchHandler := httpinterface.NewSearchHandler(searchUseCase)
	httpinterface.RegisterRoutes(e, searchHandler)

	go func() {
		log.Printf("search-service running on port %s", cfg.Port)
		if err := e.Start(":" + cfg.Port); err != nil && !errors.Is(err, http.ErrServerClosed) {
			log.Fatalf("server stopped with error: %v", err)
		}
	}()

	<-ctx.Done()
	log.Println("receive shutdown signal, cleaning...")

	if err := consumer.Close(); err != nil {
		log.Printf("close kafka consumer error: %v", err)
	}

	shutdownCtx, cancel := context.WithTimeout(context.Background(), 10*httpShutdownTimeoutUnit)
	defer cancel()
	if err := e.Shutdown(shutdownCtx); err != nil {
		log.Printf("shutdown HTTP server error: %v", err)
	}
}

const httpShutdownTimeoutUnit = 1e9
