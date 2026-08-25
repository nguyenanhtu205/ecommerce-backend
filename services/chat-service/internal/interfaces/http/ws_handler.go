package http

import (
	"context"
	"encoding/json"
	"log"
	"net/http"
	"strconv"
	"time"

	"github.com/gorilla/websocket"
	"github.com/labstack/echo/v4"

	"chat-service/internal/application"
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

type wsAttachmentInput struct {
	MediaAssetID string `json:"mediaAssetId"`
	Role         string `json:"role"`
}

type wsIncomingMessage struct {
	Content     string              `json:"content"`
	Attachments []wsAttachmentInput `json:"attachments"`
}

// Upgrade godoc
// @Summary      WebSocket endpoint for real-time chat on a conversation
// @Tags         chat
// @Param        conversationId query string true "conversationId to subscribe to"
// @Failure      403 {object} map[string]string
// @Router       /chat/ws [get]
func (h *ChatWSHandler) Upgrade(c echo.Context) error {
	ctx := c.Request().Context()
	conversationID := c.QueryParam("conversationId")
	if conversationID == "" {
		return c.JSON(http.StatusBadRequest, errResponse("conversationId is required"))
	}

	userID, shopID, role := currentUser(c)

	if err := h.usecase.AuthorizeConversationAccess(ctx, conversationID, userID, shopID, role); err != nil {
		return respondUseCaseError(c, err)
	}

	var expiresAt time.Time
	hasExpiry := false
	if expHeader := c.Request().Header.Get("X-Token-Exp"); expHeader != "" {
		if expUnix, err := strconv.ParseInt(expHeader, 10, 64); err == nil {
			expiresAt = time.Unix(expUnix, 0)
			hasExpiry = true
		}
	}

	conn, err := upgrader.Upgrade(c.Response(), c.Request(), nil)
	if err != nil {
		return err
	}

	client := h.hub.Register(conversationID, conn)
	defer h.hub.Unregister(client)

	if hasExpiry && !time.Now().Before(expiresAt) {
		closeWithTokenExpired(conn)
		return nil
	}

	var expiryTimer *time.Timer
	if hasExpiry {
		expiryTimer = time.AfterFunc(time.Until(expiresAt), func() {
			closeWithTokenExpired(conn)
		})
		defer expiryTimer.Stop()
	}

	for {
		_, raw, err := client.ReadMessage()
		if err != nil {
			break
		}

		var in wsIncomingMessage
		if err := json.Unmarshal(raw, &in); err != nil {
			continue
		}
		if in.Content == "" && len(in.Attachments) == 0 {
			continue
		}

		attachments := make([]application.MediaAttachmentInput, 0, len(in.Attachments))
		for _, a := range in.Attachments {
			attachments = append(attachments, application.MediaAttachmentInput{
				MediaAssetID: a.MediaAssetID,
				Role:         a.Role,
			})
		}

		if _, err := h.usecase.SendMessage(context.Background(), application.SendMessageInput{
			ConversationID: conversationID,
			UserID:         userID,
			ShopID:         shopID,
			SenderType:     role,
			Content:        in.Content,
			Attachments:    attachments,
		}); err != nil {
			log.Printf("chat-service: send message via ws error: %v", err)
		}
	}

	return nil
}

func closeWithTokenExpired(conn *websocket.Conn) {
	_ = conn.WriteControl(
		websocket.CloseMessage,
		websocket.FormatCloseMessage(websocket.ClosePolicyViolation, "token expired"),
		time.Now().Add(5*time.Second),
	)
	_ = conn.Close()
}
