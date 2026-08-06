package websocket

import (
	"log"
	"sync"

	"github.com/gorilla/websocket"

	"chat-service/internal/application"
)

type Client struct {
	conn           *websocket.Conn
	conversationID string
	send           chan []byte
}

type Hub struct {
	mu          sync.RWMutex
	connections map[string]map[*Client]bool
}

func NewHub() *Hub {
	return &Hub{connections: make(map[string]map[*Client]bool)}
}

func (h *Hub) Register(conversationID string, conn *websocket.Conn) *Client {
	client := &Client{conn: conn, conversationID: conversationID, send: make(chan []byte, 32)}

	h.mu.Lock()
	if h.connections[conversationID] == nil {
		h.connections[conversationID] = make(map[*Client]bool)
	}
	h.connections[conversationID][client] = true
	h.mu.Unlock()

	go client.writePump()
	return client
}

func (h *Hub) Unregister(client *Client) {
	h.mu.Lock()
	defer h.mu.Unlock()

	clients, ok := h.connections[client.conversationID]
	if !ok {
		return
	}
	if _, exists := clients[client]; !exists {
		return
	}
	delete(clients, client)
	close(client.send)
	if len(clients) == 0 {
		delete(h.connections, client.conversationID)
	}
}

func (h *Hub) PushToConversation(conversationID string, payload []byte) {
	h.mu.RLock()
	defer h.mu.RUnlock()

	for client := range h.connections[conversationID] {
		select {
		case client.send <- payload:
		default:
			log.Printf("chat-service: drop message, slow client on conversation %s", conversationID)
		}
	}
}

func (c *Client) writePump() {
	for msg := range c.send {
		if err := c.conn.WriteMessage(websocket.TextMessage, msg); err != nil {
			return
		}
	}
}

func (c *Client) ReadMessage() (int, []byte, error) {
	return c.conn.ReadMessage()
}

var _ application.RealtimePusher = (*Hub)(nil)
