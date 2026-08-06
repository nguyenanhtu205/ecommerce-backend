package http

import "github.com/labstack/echo/v4"

func RegisterRoutes(e *echo.Echo, h *ChatHandler, ws *ChatWSHandler) {
	chat := e.Group("/chat", CurrentUserMiddleware)

	chat.GET("/conversations", h.ListConversations)
	chat.POST("/conversations", h.CreateConversation)
	chat.GET("/conversations/:id/messages", h.GetMessageHistory)
	chat.POST("/conversations/:id/messages", h.SendMessage)
	chat.POST("/conversations/:id/read", h.MarkAsRead)

	chat.GET("/ws", ws.Upgrade)
}
