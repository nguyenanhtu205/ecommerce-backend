package application

import "errors"

var (
	ErrConversationNotFound = errors.New("conversation not found")

	ErrForbidden            = errors.New("forbidden: user is not a participant of this conversation")
)
