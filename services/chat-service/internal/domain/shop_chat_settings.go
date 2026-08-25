package domain

import "time"

const vacationAutoReplyMessage = "Shop hiện đang trong kỳ nghỉ và tạm thời chưa thể phản hồi tin nhắn ngay lúc này. Shop sẽ quay lại trả lời bạn sớm nhất có thể, mong bạn thông cảm nhé!"

type ShopChatSettings struct {
	ShopID            string     `bson:"_id" json:"shopId"`
	AutoReplyEnabled  bool       `bson:"autoReplyEnabled" json:"autoReplyEnabled"`
	AutoReplyMessage  string     `bson:"autoReplyMessage" json:"autoReplyMessage"`
	VacationEnabled   bool       `bson:"vacationEnabled" json:"vacationEnabled"`
	VacationStartDate *time.Time `bson:"vacationStartDate,omitempty" json:"vacationStartDate,omitempty"`
	VacationEndDate   *time.Time `bson:"vacationEndDate,omitempty" json:"vacationEndDate,omitempty"`
	VacationMessage   string     `bson:"vacationMessage" json:"vacationMessage"`
	UpdatedAt         time.Time  `bson:"updatedAt" json:"updatedAt"`
}

func (s *ShopChatSettings) IsAwayMode(now time.Time) bool {
	if s == nil {
		return false
	}
	return s.VacationEnabled && s.withinVacationRange(now)
}

func (s *ShopChatSettings) AutoReplyContent(now time.Time) string {
	if s == nil {
		return ""
	}
	if s.IsAwayMode(now) {
		return vacationAutoReplyMessage
	}
	if s.AutoReplyEnabled && s.AutoReplyMessage != "" {
		return s.AutoReplyMessage
	}
	return ""
}

func (s *ShopChatSettings) withinVacationRange(now time.Time) bool {
	if s.VacationStartDate != nil && now.Before(*s.VacationStartDate) {
		return false
	}
	if s.VacationEndDate != nil && now.After(*s.VacationEndDate) {
		return false
	}
	return true
}
