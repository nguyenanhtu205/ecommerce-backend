package middleware

import (
	"net/http"
	"strings"

	"github.com/labstack/echo/v4"
)

const (
	HeaderUserID     = "X-User-Id"
	HeaderRoles      = "X-User-Roles"
	contextKeyUserID = "auth.user_id"
	contextKeyRoles  = "auth.roles"
)

func KongAuth() echo.MiddlewareFunc {
	return func(next echo.HandlerFunc) echo.HandlerFunc {
		return func(c echo.Context) error {
			userID := c.Request().Header.Get(HeaderUserID)
			if userID == "" {
				return c.JSON(http.StatusUnauthorized, map[string]string{
					"error": "missing " + HeaderUserID + " header (request must go through Kong)",
				})
			}

			c.Set(contextKeyUserID, userID)
			c.Set(contextKeyRoles, c.Request().Header.Get(HeaderRoles))

			return next(c)
		}
	}
}

func GetUserID(c echo.Context) (string, bool) {
	v, ok := c.Get(contextKeyUserID).(string)
	return v, ok && v != ""
}

func GetRoles(c echo.Context) string {
	v, _ := c.Get(contextKeyRoles).(string)
	return v
}

// IsSeller reports whether the authenticated caller has the "seller" role,
// mirroring ICurrentUser.IsSeller in the .NET services.
func IsSeller(c echo.Context) bool {
	for r := range strings.SplitSeq(GetRoles(c), ",") {
		if strings.TrimSpace(r) == "seller" {
			return true
		}
	}
	return false
}
