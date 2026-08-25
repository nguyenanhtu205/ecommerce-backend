package httpapi

import (
	"errors"
	"fmt"
	"net/http"

	"github.com/labstack/echo/v4"

	"media-service/internal/application"
	"media-service/internal/domain"
	"media-service/internal/interfaces/http/middleware"
)

type Handler struct {
	svc *application.MediaService
}

func NewHandler(svc *application.MediaService) *Handler {
	return &Handler{svc: svc}
}

// RequestUpload godoc
//
//	@Summary		Request upload
//	@Description	Create presigned URL to upload media
//	@Tags			Upload
//	@Accept			json
//	@Produce		json
//	@Param			request	body		requestUploadRequest	true	"Upload request"
//	@Success		201		{object}	requestUploadResponse
//	@Failure		400		{object}	errorResponse
//	@Failure		500		{object}	errorResponse
//	@Router			/uploads [post]
func (h *Handler) RequestUpload(c echo.Context) error {
	var req requestUploadRequest
	if err := c.Bind(&req); err != nil {
		c.Logger().Errorf("RequestUpload bind request error: err=%v", err)
		return respondError(c, domain.ErrInvalidInput)
	}

	userID, ok := middleware.GetUserID(c)
	if !ok {
		return respondError(c, domain.ErrInvalidInput)
	}

	out, err := h.svc.RequestUpload(c.Request().Context(), application.RequestUploadInput{
		MediaType:   domain.MediaType(req.MediaType),
		ContentType: req.ContentType,
		UploadedBy:  userID,
		Checksum:    req.Checksum,
	})
	if err != nil {
		c.Logger().Errorf("RequestUpload failed: err=%v", err)
		return respondError(c, err)
	}

	return c.JSON(http.StatusCreated, requestUploadResponse{
		AssetID:       out.AssetID,
		UploadURL:     out.UploadURL,
		Bucket:        out.Bucket,
		ObjectKey:     out.ObjectKey,
		ExpiresInSecs: out.ExpiresInSecs,
	})
}

// ConfirmUpload godoc
//
//	@Summary		Confirm upload
//	@Description	Confirm upload completed and update metadata (size, duration...) for asset
//	@Tags			Upload
//	@Accept			json
//	@Produce		json
//	@Param			id		path		string					true	"Asset ID"
//	@Param			request	body		confirmUploadRequest	true	"Confirm upload request"
//	@Success		200		{object}	assetResponse
//	@Failure		400		{object}	errorResponse
//	@Failure		404		{object}	errorResponse
//	@Failure		409		{object}	errorResponse
//	@Failure		422		{object}	errorResponse
//	@Failure		500		{object}	errorResponse
//	@Router			/uploads/{id}/confirm [post]
func (h *Handler) ConfirmUpload(c echo.Context) error {
	id := c.Param("id")

	var req confirmUploadRequest
	if err := c.Bind(&req); err != nil {
		c.Logger().Errorf("ConfirmUpload bind request error: assetID=%s err=%v", id, err)
		return respondError(c, domain.ErrInvalidInput)
	}

	asset, err := h.svc.ConfirmUpload(c.Request().Context(), application.ConfirmUploadInput{
		AssetID:         id,
		Width:           req.Width,
		Height:          req.Height,
		DurationSeconds: req.DurationSeconds,
	})
	if err != nil {
		c.Logger().Errorf("ConfirmUpload failed: assetID=%s err=%v", id, err)
		return respondError(c, err)
	}

	return c.JSON(http.StatusOK, toAssetResponse(asset, ""))
}

// GetAsset godoc
//
//	@Summary		Get asset
//	@Description	Get detail information of media asset by ID
//	@Tags			Asset
//	@Produce		json
//	@Param			id	path		string	true	"Asset ID"
//	@Success		200	{object}	assetResponse
//	@Failure		404	{object}	errorResponse
//	@Failure		500	{object}	errorResponse
//	@Router			/assets/{id} [get]
func (h *Handler) GetAsset(c echo.Context) error {
	id := c.Param("id")

	asset, publicURL, err := h.svc.GetAsset(c.Request().Context(), id)
	if err != nil {
		c.Logger().Errorf("GetAsset failed: id=%s err=%v", id, err)
		return respondError(c, err)
	}

	return c.JSON(http.StatusOK, toAssetResponse(asset, publicURL))
}

// GetAssetsBulk godoc
//
//	@Summary		Get multiple assets
//	@Description	Get detail information for multiple media assets in one call. Response items preserve the exact order of the requested assetIds, including duplicates and not-found ids (found=false)
//	@Tags			Asset
//	@Accept			json
//	@Produce		json
//	@Param			payload	body		bulkAssetsRequest	true	"List of asset IDs (max 100)"
//	@Success		200		{object}	bulkAssetsResponse
//	@Failure		400		{object}	errorResponse
//	@Failure		500		{object}	errorResponse
//	@Router			/assets/bulk [post]
func (h *Handler) GetAssetsBulk(c echo.Context) error {
	var req bulkAssetsRequest
	if err := c.Bind(&req); err != nil {
		return respondError(c, domain.ErrInvalidInput)
	}
	if len(req.AssetIDs) == 0 {
		return c.JSON(http.StatusOK, bulkAssetsResponse{Items: []bulkAssetItem{}})
	}
	if len(req.AssetIDs) > 100 {
		return respondError(c, fmt.Errorf("%w: assetIds exceeds max of %d", domain.ErrInvalidInput, 100))
	}

	results, err := h.svc.GetAssets(c.Request().Context(), req.AssetIDs)
	if err != nil {
		return respondError(c, err)
	}

	items := make([]bulkAssetItem, 0, len(req.AssetIDs))
	for _, id := range req.AssetIDs {
		r, ok := results[id]
		if !ok {
			items = append(items, bulkAssetItem{ID: id, Found: false})
			continue
		}
		resp := toAssetResponse(r.Asset, r.PublicURL)
		items = append(items, bulkAssetItem{ID: id, Found: true, Asset: &resp})
	}

	return c.JSON(http.StatusOK, bulkAssetsResponse{Items: items})
}

// CreateAttachment godoc
//
//	@Summary		Create attachment
//	@Description	Assign media asset to specific entity (owner)
//	@Tags			Attachment
//	@Accept			json
//	@Produce		json
//	@Param			request	body		createAttachmentRequest	true	"Create attachment request"
//	@Success		201		{object}	attachmentResponse
//	@Failure		400		{object}	errorResponse
//	@Failure		404		{object}	errorResponse
//	@Failure		409		{object}	errorResponse
//	@Failure		500		{object}	errorResponse
//	@Router			/attachments [post]
func (h *Handler) CreateAttachment(c echo.Context) error {
	var req createAttachmentRequest
	if err := c.Bind(&req); err != nil {
		c.Logger().Errorf("CreateAttachment bind request error: %v", err)
		return respondError(c, domain.ErrInvalidInput)
	}

	att, err := h.svc.CreateAttachment(c.Request().Context(), application.CreateAttachmentInput{
		MediaAssetID: req.MediaAssetID,
		OwnerService: req.OwnerService,
		OwnerType:    req.OwnerType,
		OwnerID:      req.OwnerID,
		Role:         req.Role,
		Position:     req.Position,
	})
	if err != nil {
		c.Logger().Errorf("CreateAttachment service error: %v", err)
		return respondError(c, err)
	}

	return c.JSON(http.StatusCreated, toAttachmentResponse(att))
}

// ListAttachments godoc
//
//	@Summary		List attachments
//	@Description	Get attachment list, filter by owner_service, owner_type, owner_id, role
//	@Tags			Attachment
//	@Produce		json
//	@Param			owner_service	query		string	false	"Owner service"
//	@Param			owner_type		query		string	false	"Owner type"
//	@Param			owner_id		query		string	false	"Owner ID"
//	@Param			role			query		string	false	"Role"
//	@Success		200				{array}		attachmentResponse
//	@Failure		400				{object}	errorResponse
//	@Failure		500				{object}	errorResponse
//	@Router			/attachments [get]
func (h *Handler) ListAttachments(c echo.Context) error {
	filter := application.AttachmentFilter{
		OwnerService: c.QueryParam("owner_service"),
		OwnerType:    c.QueryParam("owner_type"),
		OwnerID:      c.QueryParam("owner_id"),
		Role:         c.QueryParam("role"),
	}

	list, err := h.svc.ListAttachments(c.Request().Context(), filter)
	if err != nil {
		c.Logger().Errorf(
			"ListAttachments failed: ownerService=%s ownerType=%s ownerID=%s role=%s err=%v",
			filter.OwnerService,
			filter.OwnerType,
			filter.OwnerID,
			filter.Role,
			err,
		)
		return respondError(c, err)
	}

	resp := make([]attachmentResponse, 0, len(list))
	for _, a := range list {
		resp = append(resp, toAttachmentResponse(a))
	}

	return c.JSON(http.StatusOK, resp)
}

// DeleteAttachment godoc
//
//	@Summary		Delete attachment
//	@Description	Delete attachment by ID
//	@Tags			Attachment
//	@Param			id	path	string	true	"Attachment ID"
//	@Success		204	"No Content"
//	@Failure		404	{object}	errorResponse
//	@Failure		500	{object}	errorResponse
//	@Router			/attachments/{id} [delete]
func (h *Handler) DeleteAttachment(c echo.Context) error {
	id := c.Param("id")

	if err := h.svc.DeleteAttachment(c.Request().Context(), id); err != nil {
		c.Logger().Errorf("DeleteAttachment failed: id=%s, err=%v", id, err)
		return respondError(c, err)
	}

	return c.NoContent(http.StatusNoContent)
}

// GetAssetsByOwnerRoleBulk godoc
//
//	@Summary		Get assets by owner+role
//	@Description	Resolve multiple (ownerId, role) pairs to their currently-attached asset in one call — e.g. fetch the thumbnail for many products on a listing page. Response items preserve the exact order of the requested items, including pairs that resolve to nothing (found=false)
//	@Tags			Asset
//	@Accept			json
//	@Produce		json
//	@Param			payload	body		bulkAssetsByOwnerRoleRequest	true	"Owner service/type + list of (ownerId, role) pairs (max 100)"
//	@Success		200		{object}	bulkAssetsByOwnerRoleResponse
//	@Failure		400		{object}	errorResponse
//	@Failure		500		{object}	errorResponse
//	@Router			/assets/by-owner-role/bulk [post]
func (h *Handler) GetAssetsByOwnerRoleBulk(c echo.Context) error {
	var req bulkAssetsByOwnerRoleRequest
	if err := c.Bind(&req); err != nil {
		return respondError(c, domain.ErrInvalidInput)
	}
	if len(req.Items) == 0 {
		return c.JSON(http.StatusOK, bulkAssetsByOwnerRoleResponse{Items: []bulkAssetsByOwnerRoleItem{}})
	}
	if len(req.Items) > 100 {
		return respondError(c, fmt.Errorf("%w: items exceeds max of %d", domain.ErrInvalidInput, 100))
	}

	pairs := make([]application.OwnerRolePair, 0, len(req.Items))
	for _, item := range req.Items {
		pairs = append(pairs, application.OwnerRolePair{OwnerID: item.OwnerID, Role: item.Role})
	}

	results, err := h.svc.GetAssetsByOwnerRole(c.Request().Context(), req.OwnerService, req.OwnerType, pairs)
	if err != nil {
		return respondError(c, err)
	}

	items := make([]bulkAssetsByOwnerRoleItem, 0, len(results))
	for _, r := range results {
		if !r.Found {
			items = append(items, bulkAssetsByOwnerRoleItem{OwnerID: r.OwnerID, Role: r.Role, Found: false})
			continue
		}
		resp := toAssetResponse(r.Asset, r.PublicURL)
		items = append(items, bulkAssetsByOwnerRoleItem{OwnerID: r.OwnerID, Role: r.Role, Found: true, Asset: &resp})
	}

	return c.JSON(http.StatusOK, bulkAssetsByOwnerRoleResponse{Items: items})
}

func toAssetResponse(a *domain.MediaAsset, publicURL string) assetResponse {
	return assetResponse{
		ID:              a.ID,
		Bucket:          a.Bucket,
		ObjectKey:       a.ObjectKey,
		MediaType:       string(a.MediaType),
		ContentType:     a.ContentType,
		SizeBytes:       a.SizeBytes,
		Width:           a.Width,
		Height:          a.Height,
		DurationSeconds: a.DurationSeconds,
		Status:          string(a.Status),
		UploadedBy:      a.UploadedBy,
		CreatedAt:       a.CreatedAt,
		PublicURL:       publicURL,
	}
}

func toAttachmentResponse(a *domain.MediaAttachment) attachmentResponse {
	return attachmentResponse{
		ID:           a.ID,
		MediaAssetID: a.MediaAssetID,
		OwnerService: a.OwnerService,
		OwnerType:    a.OwnerType,
		OwnerID:      a.OwnerID,
		Role:         a.Role,
		Position:     a.Position,
		CreatedAt:    a.CreatedAt,
		UpdatedAt:    a.UpdatedAt,
	}
}

func respondError(c echo.Context, err error) error {
	switch {
	case errors.Is(err, domain.ErrNotFound):
		return c.JSON(http.StatusNotFound, errorResponse{Error: err.Error()})
	case errors.Is(err, domain.ErrInvalidInput):
		return c.JSON(http.StatusBadRequest, errorResponse{Error: err.Error()})
	case errors.Is(err, domain.ErrConflict):
		return c.JSON(http.StatusConflict, errorResponse{Error: err.Error()})
	case errors.Is(err, domain.ErrUploadNotReady):
		return c.JSON(http.StatusUnprocessableEntity, errorResponse{Error: err.Error()})
	default:
		return c.JSON(http.StatusInternalServerError, errorResponse{Error: "internal server error"})
	}
}
