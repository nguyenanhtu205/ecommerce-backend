// @title       Cart Service API
// @version     1.0
package main

import (
	"log"
	"net/http"

	"github.com/joho/godotenv"
	"github.com/labstack/echo/v4"
	"github.com/labstack/echo/v4/middleware"
	goredis "github.com/redis/go-redis/v9"
	echoSwagger "github.com/swaggo/echo-swagger"

	_ "cart-service/docs"

	"cart-service/config"
	"cart-service/internal/application"
	redisinfra "cart-service/internal/infrastructure/redis"
	httpif "cart-service/internal/interfaces/http"
)

func main() {
	_ = godotenv.Load()

	cfg := config.Load()

	client := goredis.NewClient(&goredis.Options{
		Addr: cfg.RedisAddr,
	})

	repo := redisinfra.NewCartRepository(client)
	uc := application.NewCartUseCase(repo)
	handler := httpif.NewCartHandler(uc)

	e := echo.New()
	e.GET("/", func(c echo.Context) error {
		return c.Redirect(http.StatusMovedPermanently, "/swagger/index.html")
	})
	e.GET("/swagger/*", echoSwagger.WrapHandler)
	e.Use(middleware.RequestLogger())
	e.Use(middleware.Recover())

	httpif.RegisterRoutes(e, handler)

	log.Printf("cart-service running on port %s", cfg.Port)
	log.Fatal(e.Start(":" + cfg.Port))
}
