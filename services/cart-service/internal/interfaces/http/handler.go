package http

import (
	"errors"
	"net/http"

	"github.com/labstack/echo/v4"

	"cart-service/internal/application"
)

type CartHandler struct {
	usecase *application.CartUseCase
}

func NewCartHandler(usecase *application.CartUseCase) *CartHandler {
	return &CartHandler{usecase: usecase}
}

func errResponse(msg string) map[string]string {
	return map[string]string{"error": msg}
}

func userIdFromRequest(c echo.Context) (string, error) {
	userId := c.Request().Header.Get("X-User-Id")
	if userId == "" {
		return "", errors.New("missing X-User-Id header")
	}
	return userId, nil
}

// AddItem godoc
// @Summary      Add item to cart
// @Description  If combinationId already exists in the cart, quantity is added on top of the existing quantity.
// @Tags         cart
// @Accept       json
// @Produce      json
// @Param        body body application.AddItemRequest true "item to add"
// @Success      201
// @Failure      400 {object} map[string]string
// @Failure      401 {object} map[string]string
// @Router       /cart/items [post]
func (h *CartHandler) AddItem(c echo.Context) error {
	ctx := c.Request().Context()

	userId, err := userIdFromRequest(c)
	if err != nil {
		return c.JSON(http.StatusUnauthorized, errResponse(err.Error()))
	}

	var req application.AddItemRequest
	if err := c.Bind(&req); err != nil {
		return c.JSON(http.StatusBadRequest, errResponse(err.Error()))
	}
	if req.CombinationId == "" || req.Quantity <= 0 {
		return c.JSON(http.StatusBadRequest, errResponse("combinationId and a positive quantity are required"))
	}

	if err := h.usecase.AddItem(ctx, userId, req); err != nil {
		return c.JSON(http.StatusInternalServerError, errResponse("add item failed"))
	}

	return c.NoContent(http.StatusCreated)
}

// UpdateItem godoc
// @Summary      Update item quantity or selection
// @Tags         cart
// @Accept       json
// @Produce      json
// @Param        combinationId path string true "combination id"
// @Param        body body application.UpdateItemRequest true "fields to update"
// @Success      200
// @Failure      404 {object} map[string]string
// @Router       /cart/items/{combinationId} [patch]
func (h *CartHandler) UpdateItem(c echo.Context) error {
	ctx := c.Request().Context()

	userId, err := userIdFromRequest(c)
	if err != nil {
		return c.JSON(http.StatusUnauthorized, errResponse(err.Error()))
	}

	combinationId := c.Param("combinationId")

	var req application.UpdateItemRequest
	if err := c.Bind(&req); err != nil {
		return c.JSON(http.StatusBadRequest, errResponse(err.Error()))
	}

	if err := h.usecase.UpdateItem(ctx, userId, combinationId, req); err != nil {
		if errors.Is(err, application.ErrItemNotFound) {
			return c.JSON(http.StatusNotFound, errResponse(err.Error()))
		}
		return c.JSON(http.StatusInternalServerError, errResponse("update item failed"))
	}

	return c.NoContent(http.StatusOK)
}

// RemoveItem godoc
// @Summary      Remove one item from cart
// @Tags         cart
// @Param        combinationId path string true "combination id"
// @Success      204
// @Router       /cart/items/{combinationId} [delete]
func (h *CartHandler) RemoveItem(c echo.Context) error {
	ctx := c.Request().Context()

	userId, err := userIdFromRequest(c)
	if err != nil {
		return c.JSON(http.StatusUnauthorized, errResponse(err.Error()))
	}

	combinationId := c.Param("combinationId")

	if err := h.usecase.RemoveItem(ctx, userId, combinationId); err != nil {
		return c.JSON(http.StatusInternalServerError, errResponse("remove item failed"))
	}

	return c.NoContent(http.StatusNoContent)
}

// RemoveItems godoc
// @Summary      Remove multiple items from cart at once
// @Tags         cart
// @Accept       json
// @Param        body body application.RemoveItemsRequest true "combination ids to remove"
// @Success      204
// @Router       /cart/items [delete]
func (h *CartHandler) RemoveItems(c echo.Context) error {
	ctx := c.Request().Context()

	userId, err := userIdFromRequest(c)
	if err != nil {
		return c.JSON(http.StatusUnauthorized, errResponse(err.Error()))
	}

	var req application.RemoveItemsRequest
	if err := c.Bind(&req); err != nil {
		return c.JSON(http.StatusBadRequest, errResponse(err.Error()))
	}
	if len(req.CombinationIds) == 0 {
		return c.JSON(http.StatusBadRequest, errResponse("combinationIds must not be empty"))
	}

	if err := h.usecase.RemoveItems(ctx, userId, req.CombinationIds); err != nil {
		return c.JSON(http.StatusInternalServerError, errResponse("remove items failed"))
	}

	return c.NoContent(http.StatusNoContent)
}

// GetCart godoc
// @Summary      Get full cart grouped by shop
// @Tags         cart
// @Produce      json
// @Success      200 {array} application.ShopCartGroup
// @Router       /cart [get]
func (h *CartHandler) GetCart(c echo.Context) error {
	ctx := c.Request().Context()

	userId, err := userIdFromRequest(c)
	if err != nil {
		return c.JSON(http.StatusUnauthorized, errResponse(err.Error()))
	}

	groups, err := h.usecase.GetCart(ctx, userId)
	if err != nil {
		return c.JSON(http.StatusInternalServerError, errResponse("get cart failed"))
	}

	return c.JSON(http.StatusOK, groups)
}

// ClearCart godoc
// @Summary      Clear the entire cart (used after checkout succeeds)
// @Tags         cart
// @Success      204
// @Router       /cart [delete]
func (h *CartHandler) ClearCart(c echo.Context) error {
	ctx := c.Request().Context()

	userId, err := userIdFromRequest(c)
	if err != nil {
		return c.JSON(http.StatusUnauthorized, errResponse(err.Error()))
	}

	if err := h.usecase.ClearCart(ctx, userId); err != nil {
		return c.JSON(http.StatusInternalServerError, errResponse("clear cart failed"))
	}

	return c.NoContent(http.StatusNoContent)
}

// GetSelectedSummary godoc
// @Summary      Get selected items grouped by shop, for building CheckoutCommand
// @Description  priceSnapshot is intentionally omitted from this response — Order Service always re-fetches real price from Inventory Service at checkout.
// @Tags         cart
// @Produce      json
// @Success      200 {array} application.ShopCheckoutInfo
// @Router       /cart/selected-summary [get]
func (h *CartHandler) GetSelectedSummary(c echo.Context) error {
	ctx := c.Request().Context()

	userId, err := userIdFromRequest(c)
	if err != nil {
		return c.JSON(http.StatusUnauthorized, errResponse(err.Error()))
	}

	summary, err := h.usecase.GetSelectedSummary(ctx, userId)
	if err != nil {
		return c.JSON(http.StatusInternalServerError, errResponse("get selected summary failed"))
	}

	return c.JSON(http.StatusOK, summary)
}
