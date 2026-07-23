package domain

import "time"

const (
	RoleAvatar    = "avatar"
	RoleThumbnail = "thumbnail"
	RoleCover     = "cover"
	RoleGallery   = "gallery"
	RoleVideo     = "video"
)

func IsUniqueRole(role string) bool {
	switch role {
	case RoleAvatar, RoleThumbnail, RoleCover:
		return true
	}
	return false
}

type MediaAttachment struct {
	ID           string
	MediaAssetID string
	OwnerService string
	OwnerType    string
	OwnerID      string
	Role         string
	Position     int
	CreatedAt    time.Time
	UpdatedAt    *time.Time
	DeletedAt    *time.Time
}
