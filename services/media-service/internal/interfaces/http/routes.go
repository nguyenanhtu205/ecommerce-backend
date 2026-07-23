package httpapi

import "github.com/labstack/echo/v4"

func RegisterRoutes(e *echo.Echo, h *Handler) {
	g := e.Group("/media")

	g.POST("/uploads", h.RequestUpload)
	g.POST("/uploads/:id/confirm", h.ConfirmUpload)
	g.GET("/assets/:id", h.GetAsset)

	g.POST("/attachments", h.CreateAttachment)
	g.GET("/attachments", h.ListAttachments)
	g.DELETE("/attachments/:id", h.DeleteAttachment)
}
