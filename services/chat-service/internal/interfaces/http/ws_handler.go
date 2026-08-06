package http

import (
	"context"
	"encoding/json"
	"log"
	"net/http"

	"github.com/gorilla/websocket"
	"github.com/labstack/echo/v4"

	"chat-service/internal/application"
	"chat-service/internal/domain"
	wsinfra "chat-service/internal/infrastructure/websocket"
)

var upgrader = websocket.Upgrader{
	CheckOrigin: func(r *http.Request) bool { return true },
}

type ChatWSHandler struct {
	hub     *wsinfra.Hub
	usecase *application.ChatUseCase
}

func NewChatWSHandler(hub *wsinfra.Hub, usecase *application.ChatUseCase) *ChatWSHandler {
	return &ChatWSHandler{hub: hub, usecase: usecase}
}

type wsIncomingMessage struct {
	Content       string   `json:"content"`
	AttachmentIDs []string `json:"attachmentMediaAssetIds"`
}

// Upgrade godoc
// @Summary      WebSocket endpoint for real-time chat on a conversation — used only to push/receive
//
//	messages; REST remains the source of truth (this handler calls the same SendMessage
//	use case as the REST endpoint).
//
// @Tags         chat
// @Param        conversationId query string true "conversationId to subscribe to"
// @Param        role           query string true "buyer or shop"
// @Router       /chat/ws [get]
func (h *ChatWSHandler) Upgrade(c echo.Context) error {
	conversationID := c.QueryParam("conversationId")
	role := domain.SenderType(c.QueryParam("role"))
	if conversationID == "" || (role != domain.SenderBuyer && role != domain.SenderShop) {
		return c.JSON(http.StatusBadRequest, errResponse("conversationId and a valid role are required"))
	}

	conn, err := upgrader.Upgrade(c.Response(), c.Request(), nil)
	if err != nil {
		return err
	}

	client := h.hub.Register(conversationID, conn)
	defer h.hub.Unregister(client)

	for {
		_, raw, err := client.ReadMessage()
		if err != nil {
			break
		}

		var in wsIncomingMessage
		if err := json.Unmarshal(raw, &in); err != nil {
			continue
		}
		if in.Content == "" {
			continue
		}

		if _, err := h.usecase.SendMessage(context.Background(), application.SendMessageInput{
			ConversationID: conversationID,
			SenderType:     role,
			Content:        in.Content,
			AttachmentIDs:  in.AttachmentIDs,
		}); err != nil {
			log.Printf("chat-service: send message via ws error: %v", err)
		}
	}

	return nil
}
