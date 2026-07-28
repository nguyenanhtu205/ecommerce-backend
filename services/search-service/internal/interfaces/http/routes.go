package http

import (
	"github.com/labstack/echo/v4"
)

func RegisterRoutes(e *echo.Echo, h *SearchHandler) {
	g := e.Group("/search")

	g.GET("", h.Search)
	g.GET("/suggest", h.Suggest)
	g.GET("/trending", h.Trending)
}
