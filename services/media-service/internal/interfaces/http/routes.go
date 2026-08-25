package httpapi

import "github.com/labstack/echo/v4"

func RegisterRoutes(e *echo.Echo, h *Handler, authMiddleware echo.MiddlewareFunc) {
	g := e.Group("/media", authMiddleware)

	g.POST("/uploads", h.RequestUpload)
	g.POST("/uploads/:id/confirm", h.ConfirmUpload)

	g.POST("/attachments", h.CreateAttachment)
	g.GET("/attachments", h.ListAttachments)
	g.DELETE("/attachments/:id", h.DeleteAttachment)

	e.POST("/media/assets/bulk", h.GetAssetsBulk)
	e.POST("/media/assets/by-owner-role/bulk", h.GetAssetsByOwnerRoleBulk)
	e.GET("/media/assets/:id", h.GetAsset)
}
