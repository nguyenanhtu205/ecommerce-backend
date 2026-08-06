package http

import (
	"net/http"
	"strconv"

	"github.com/labstack/echo/v4"

	"chat-service/internal/application"
	"chat-service/internal/domain"
)

type ChatHandler struct {
	usecase *application.ChatUseCase
}

func NewChatHandler(usecase *application.ChatUseCase) *ChatHandler {
	return &ChatHandler{usecase: usecase}
}

type createConversationRequest struct {
	BuyerID string `json:"buyerId"`
	ShopID  string `json:"shopId"`
}

type sendMessageRequest struct {
	Content       string   `json:"content"`
	AttachmentIDs []string `json:"attachmentMediaAssetIds"`
}

func errResponse(msg string) map[string]string {
	return map[string]string{"error": msg}
}

// ListConversations godoc
// @Summary      List conversations of the current user
// @Tags         chat
// @Param        role query string true "buyer or shop"
// @Success      200 {array} domain.Conversation
// @Router       /chat/conversations [get]
func (h *ChatHandler) ListConversations(c echo.Context) error {
	ctx := c.Request().Context()
	userID, _ := c.Get("userId").(string)

	role := domain.SenderType(c.QueryParam("role"))
	if role != domain.SenderBuyer && role != domain.SenderShop {
		return c.JSON(http.StatusBadRequest, errResponse("role must be buyer or shop"))
	}

	conversations, err := h.usecase.ListConversations(ctx, userID, role)
	if err != nil {
		return c.JSON(http.StatusInternalServerError, errResponse("list conversations failed"))
	}

	return c.JSON(http.StatusOK, conversations)
}

// CreateConversation godoc
// @Summary      Create or get a buyer-shop conversation (idempotent)
// @Tags         chat
// @Accept       json
// @Param        body body createConversationRequest true "buyerId + shopId"
// @Success      200 {object} domain.Conversation
// @Router       /chat/conversations [post]
func (h *ChatHandler) CreateConversation(c echo.Context) error {
	ctx := c.Request().Context()

	var req createConversationRequest
	if err := c.Bind(&req); err != nil {
		return c.JSON(http.StatusBadRequest, errResponse("invalid request body"))
	}
	if req.BuyerID == "" || req.ShopID == "" {
		return c.JSON(http.StatusBadRequest, errResponse("buyerId and shopId are required"))
	}

	conv, err := h.usecase.GetOrCreateConversation(ctx, req.BuyerID, req.ShopID)
	if err != nil {
		return c.JSON(http.StatusInternalServerError, errResponse("create conversation failed"))
	}

	return c.JSON(http.StatusOK, conv)
}

// GetMessageHistory godoc
// @Summary      Paginated message history of a conversation
// @Tags         chat
// @Param        id   path  string true "conversationId"
// @Param        page query int    false "page number, starting from 1"
// @Success      200 {array} domain.Message
// @Router       /chat/conversations/{id}/messages [get]
func (h *ChatHandler) GetMessageHistory(c echo.Context) error {
	ctx := c.Request().Context()
	conversationID := c.Param("id")

	page := 1
	if v := c.QueryParam("page"); v != "" {
		if parsed, err := strconv.Atoi(v); err == nil {
			page = parsed
		}
	}

	messages, err := h.usecase.GetMessageHistory(ctx, conversationID, page)
	if err != nil {
		return c.JSON(http.StatusInternalServerError, errResponse("get message history failed"))
	}

	return c.JSON(http.StatusOK, messages)
}

// SendMessage godoc
// @Summary      Send a message via REST — also the entrypoint reused by the WebSocket handler
// @Tags         chat
// @Accept       json
// @Param        id   path string true "conversationId"
// @Param        body body sendMessageRequest true "message content"
// @Success      201 {object} domain.Message
// @Router       /chat/conversations/{id}/messages [post]
func (h *ChatHandler) SendMessage(c echo.Context) error {
	ctx := c.Request().Context()
	conversationID := c.Param("id")

	var req sendMessageRequest
	if err := c.Bind(&req); err != nil {
		return c.JSON(http.StatusBadRequest, errResponse("invalid request body"))
	}
	if req.Content == "" {
		return c.JSON(http.StatusBadRequest, errResponse("content must not be empty"))
	}

	role, _ := c.Get("role").(string)
	if role == "" {
		return c.JSON(http.StatusUnauthorized, errResponse("missing role in context, check auth middleware"))
	}

	msg, err := h.usecase.SendMessage(ctx, application.SendMessageInput{
		ConversationID: conversationID,
		SenderType:     domain.SenderType(role),
		Content:        req.Content,
		AttachmentIDs:  req.AttachmentIDs,
	})
	if err != nil {
		return c.JSON(http.StatusInternalServerError, errResponse("send message failed"))
	}

	return c.JSON(http.StatusCreated, msg)
}

// MarkAsRead godoc
// @Summary      Mark all messages from the other party as read
// @Tags         chat
// @Param        id path string true "conversationId"
// @Success      204
// @Router       /chat/conversations/{id}/read [post]
func (h *ChatHandler) MarkAsRead(c echo.Context) error {
	ctx := c.Request().Context()
	conversationID := c.Param("id")

	role, _ := c.Get("role").(string)
	if role == "" {
		return c.JSON(http.StatusUnauthorized, errResponse("missing role in context, check auth middleware"))
	}

	if err := h.usecase.MarkAsRead(ctx, conversationID, domain.SenderType(role)); err != nil {
		return c.JSON(http.StatusInternalServerError, errResponse("mark as read failed"))
	}

	return c.NoContent(http.StatusNoContent)
}
