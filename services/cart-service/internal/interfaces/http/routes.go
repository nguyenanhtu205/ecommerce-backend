package http

import "github.com/labstack/echo/v4"

func RegisterRoutes(e *echo.Echo, h *CartHandler) {
	cart := e.Group("/cart")

	cart.POST("/items", h.AddItem)
	cart.PATCH("/items/:combinationId", h.UpdateItem)
	cart.DELETE("/items/:combinationId", h.RemoveItem)
	cart.DELETE("/items", h.RemoveItems)
	cart.GET("", h.GetCart)
	cart.DELETE("", h.ClearCart)
	cart.GET("/selected-summary", h.GetSelectedSummary)
}
