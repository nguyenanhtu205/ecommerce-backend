package http

import (
	"errors"
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
	ShopID string `json:"shopId"`
}

type attachmentInput struct {
	MediaAssetID string `json:"mediaAssetId"`
	Role         string `json:"role"`
}

type sendMessageRequest struct {
	Content     string            `json:"content"`
	Attachments []attachmentInput `json:"attachments"`
}

func errResponse(msg string) map[string]string {
	return map[string]string{"error": msg}
}

func respondUseCaseError(c echo.Context, err error) error {
	switch {
	case errors.Is(err, application.ErrForbidden):
		return c.JSON(http.StatusForbidden, errResponse("you are not a participant of this conversation"))
	case errors.Is(err, application.ErrConversationNotFound):
		return c.JSON(http.StatusNotFound, errResponse("conversation not found"))
	default:
		return c.JSON(http.StatusInternalServerError, errResponse("internal error"))
	}
}

func currentUser(c echo.Context) (userID, shopID string, role domain.SenderType) {
	userID, _ = c.Get("userId").(string)
	shopID, _ = c.Get("shopId").(string)
	roleStr, _ := c.Get("role").(string)
	return userID, shopID, domain.SenderType(roleStr)
}

// ListConversations godoc
// @Summary      List conversations of the current user
// @Tags         chat
// @Success      200 {array} domain.Conversation
// @Router       /chat/conversations [get]
func (h *ChatHandler) ListConversations(c echo.Context) error {
	ctx := c.Request().Context()
	userID, shopID, role := currentUser(c)

	conversations, err := h.usecase.ListConversations(ctx, userID, shopID, role)
	if err != nil {
		return c.JSON(http.StatusInternalServerError, errResponse("list conversations failed"))
	}

	return c.JSON(http.StatusOK, conversations)
}

// CreateConversation godoc
// @Summary      Buyer starts (or reuses) a conversation with a shop — buyerId is taken from
//
//	the authenticated user, not from the request body
//
// @Tags         chat
// @Accept       json
// @Param        body body createConversationRequest true "shopId"
// @Success      200 {object} domain.Conversation
// @Router       /chat/conversations [post]
func (h *ChatHandler) CreateConversation(c echo.Context) error {
	ctx := c.Request().Context()
	userID, _, role := currentUser(c)

	if role != domain.SenderBuyer {
		return c.JSON(http.StatusForbidden, errResponse("only a buyer can start a conversation"))
	}

	var req createConversationRequest
	if err := c.Bind(&req); err != nil {
		return c.JSON(http.StatusBadRequest, errResponse("invalid request body"))
	}
	if req.ShopID == "" {
		return c.JSON(http.StatusBadRequest, errResponse("shopId is required"))
	}

	conv, err := h.usecase.GetOrCreateConversation(ctx, userID, req.ShopID)
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
// @Failure      403 {object} map[string]string
// @Router       /chat/conversations/{id}/messages [get]
func (h *ChatHandler) GetMessageHistory(c echo.Context) error {
	ctx := c.Request().Context()
	conversationID := c.Param("id")
	userID, shopID, role := currentUser(c)

	page := 1
	if v := c.QueryParam("page"); v != "" {
		if parsed, err := strconv.Atoi(v); err == nil {
			page = parsed
		}
	}

	messages, err := h.usecase.GetMessageHistory(ctx, conversationID, userID, shopID, role, page)
	if err != nil {
		return respondUseCaseError(c, err)
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
// @Failure      403 {object} map[string]string
// @Router       /chat/conversations/{id}/messages [post]
func (h *ChatHandler) SendMessage(c echo.Context) error {
	ctx := c.Request().Context()
	conversationID := c.Param("id")
	userID, shopID, role := currentUser(c)

	var req sendMessageRequest
	if err := c.Bind(&req); err != nil {
		return c.JSON(http.StatusBadRequest, errResponse("invalid request body"))
	}
	if req.Content == "" && len(req.Attachments) == 0 {
		return c.JSON(http.StatusBadRequest, errResponse("message must have content or at least one attachment"))
	}

	attachments := make([]application.MediaAttachmentInput, 0, len(req.Attachments))
	for _, a := range req.Attachments {
		attachments = append(attachments, application.MediaAttachmentInput{
			MediaAssetID: a.MediaAssetID,
			Role:         a.Role,
		})
	}

	msg, err := h.usecase.SendMessage(ctx, application.SendMessageInput{
		ConversationID: conversationID,
		UserID:         userID,
		ShopID:         shopID,
		SenderType:     role,
		Content:        req.Content,
		Attachments:    attachments,
	})
	if err != nil {
		return respondUseCaseError(c, err)
	}

	return c.JSON(http.StatusCreated, msg)
}

// MarkAsRead godoc
// @Summary      Mark all messages from the other party as read
// @Tags         chat
// @Param        id path string true "conversationId"
// @Success      204
// @Failure      403 {object} map[string]string
// @Router       /chat/conversations/{id}/read [post]
func (h *ChatHandler) MarkAsRead(c echo.Context) error {
	ctx := c.Request().Context()
	conversationID := c.Param("id")
	userID, shopID, role := currentUser(c)

	if err := h.usecase.MarkAsRead(ctx, conversationID, userID, shopID, role); err != nil {
		return respondUseCaseError(c, err)
	}

	return c.NoContent(http.StatusNoContent)
}
