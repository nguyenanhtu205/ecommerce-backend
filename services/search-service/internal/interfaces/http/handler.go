package http

import (
	"net/http"
	"strconv"

	"github.com/labstack/echo/v4"

	"search-service/internal/application"
)

type SearchHandler struct {
	usecase *application.SearchUseCase
}

func NewSearchHandler(usecase *application.SearchUseCase) *SearchHandler {
	return &SearchHandler{usecase: usecase}
}

// Search godoc
// @Summary      Full-text search products
// @Tags         search
// @Param        q          query string false "search keyword"
// @Param        priceMin   query int    false "minimum price"
// @Param        priceMax   query int    false "maximum price"
// @Param        category   query string false "category id"
// @Param        sort       query string false "price|rating|soldCount, append :asc or :desc, e.g. price:asc"
// @Param        page       query int    false "page number, starting from 1"
// @Param        size       query int    false "items per page"
// @Param        location   query string false "location filter"
// @Success      200 {object} SearchResponse
// @Router       /search [get]
func (h *SearchHandler) Search(c echo.Context) error {
	ctx := c.Request().Context()
	q := c.QueryParam("q")

	filters := application.SearchFilters{
		Category: c.QueryParam("category"),
		Location: c.QueryParam("location"),
	}
	if v := c.QueryParam("priceMin"); v != "" {
		if parsed, err := strconv.ParseInt(v, 10, 64); err == nil {
			filters.PriceMin = &parsed
		} else {
			return c.JSON(http.StatusBadRequest, errResponse("invalid priceMin"))
		}
	}

	if v := c.QueryParam("priceMax"); v != "" {
		if parsed, err := strconv.ParseInt(v, 10, 64); err == nil {
			filters.PriceMax = &parsed
		} else {
			return c.JSON(http.StatusBadRequest, errResponse("invalid priceMax"))
		}
	}

	sort, err := parseSort(c.QueryParam("sort"))
	if err != nil {
		return c.JSON(http.StatusBadRequest, errResponse(err.Error()))
	}

	page := application.Page{}
	if v := c.QueryParam("page"); v != "" {
		if parsed, err := strconv.Atoi(v); err == nil {
			page.Number = parsed
		}
	}
	if v := c.QueryParam("size"); v != "" {
		if parsed, err := strconv.Atoi(v); err == nil {
			page.Size = parsed
		}
	}

	result, err := h.usecase.Search(ctx, q, filters, sort, page)
	if err != nil {
		return c.JSON(http.StatusInternalServerError, errResponse("search failed"))
	}

	return c.JSON(http.StatusOK, SearchResponse{
		Total: result.Total,
		Items: result.Items,
	})
}

// Suggest godoc
// @Summary      Autocomplete product name
// @Tags         search
// @Param        q query string true "prefix to suggest against"
// @Success      200 {object} SuggestResponse
// @Router       /search/suggest [get]
func (h *SearchHandler) Suggest(c echo.Context) error {
	ctx := c.Request().Context()
	q := c.QueryParam("q")

	suggestions, err := h.usecase.Suggest(ctx, q)
	if err != nil {
		return c.JSON(http.StatusInternalServerError, errResponse("suggest failed"))
	}

	return c.JSON(http.StatusOK, SuggestResponse{Suggestions: suggestions})
}

func errResponse(msg string) map[string]string {
	return map[string]string{"error": msg}
}

// Trending godoc
// @Summary      Top trending search keywords
// @Tags         search
// @Param        limit query int false "number of keywords to return"
// @Success      200 {object} TrendingResponse
// @Router       /search/trending [get]
func (h *SearchHandler) Trending(c echo.Context) error {
	ctx := c.Request().Context()

	limit := 10
	if v := c.QueryParam("limit"); v != "" {
		if parsed, err := strconv.Atoi(v); err == nil {
			limit = parsed
		}
	}

	keywords, err := h.usecase.GetTrending(ctx, limit)
	if err != nil {
		return c.JSON(http.StatusInternalServerError, errResponse("get trending failed"))
	}

	items := make([]TrendingItem, 0, len(keywords))
	for _, k := range keywords {
		items = append(items, TrendingItem{Keyword: k.Keyword, Score: k.Score})
	}

	return c.JSON(http.StatusOK, TrendingResponse{Items: items})
}
