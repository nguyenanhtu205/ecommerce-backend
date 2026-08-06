package domain

import "time"

type SenderType string

const (
	SenderBuyer SenderType = "buyer"
	SenderShop  SenderType = "shop"
)

type Conversation struct {
	ID                string    `bson:"_id" json:"id"`
	BuyerID           string    `bson:"buyerId" json:"buyerId"`
	ShopID            string    `bson:"shopId" json:"shopId"`
	LastMessage       string    `bson:"lastMessage" json:"lastMessage"`
	LastMessageAt     time.Time `bson:"lastMessageAt" json:"lastMessageAt"`
	BuyerUnreadCount  int       `bson:"buyerUnreadCount" json:"buyerUnreadCount"`
	SellerUnreadCount int       `bson:"sellerUnreadCount" json:"sellerUnreadCount"`
	CreatedAt         time.Time `bson:"createdAt" json:"createdAt"`
}

type Message struct {
	ID                      string     `bson:"_id" json:"id"`
	ConversationID          string     `bson:"conversationId" json:"conversationId"`
	SenderType              SenderType `bson:"senderType" json:"senderType"`
	Content                 string     `bson:"content" json:"content"`
	AttachmentMediaAssetIDs []string   `bson:"attachmentMediaAssetIds" json:"attachmentMediaAssetIds"`
	IsRead                  bool       `bson:"isRead" json:"isRead"`
	CreatedAt               time.Time  `bson:"createdAt" json:"createdAt"`
}
