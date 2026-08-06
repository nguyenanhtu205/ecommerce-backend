package http

import (
	"net/http"
	"strings"

	"github.com/labstack/echo/v4"

	"chat-service/internal/domain"
)

func CurrentUserMiddleware(next echo.HandlerFunc) echo.HandlerFunc {
	return func(c echo.Context) error {
		userID := c.Request().Header.Get("X-User-Id")
		if userID == "" {
			return c.JSON(http.StatusUnauthorized, errResponse("missing X-User-Id header"))
		}

		role := domain.SenderBuyer
		rolesHeader := c.Request().Header.Get("X-User-Roles")
		for r := range strings.SplitSeq(rolesHeader, ",") {
			if strings.TrimSpace(r) == "seller" {
				role = domain.SenderShop
				break
			}
		}

		c.Set("userId", userID)
		c.Set("role", string(role))

		return next(c)
	}
}
