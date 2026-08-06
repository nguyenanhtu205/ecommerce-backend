package kafka

import (
	"context"
	"encoding/json"
	"fmt"

	kafkago "github.com/segmentio/kafka-go"

	"chat-service/internal/application"
)

type Publisher struct {
	writer *kafkago.Writer
}

func NewPublisher(brokers []string, topic string) *Publisher {
	writer := &kafkago.Writer{
		Addr:     kafkago.TCP(brokers...),
		Topic:    topic,
		Balancer: &kafkago.LeastBytes{},
	}

	return &Publisher{writer: writer}
}

func (p *Publisher) PublishMessageSent(ctx context.Context, event application.MessageSentEvent) error {
	payload, err := json.Marshal(event)
	if err != nil {
		return fmt.Errorf("marshal MessageSent event: %w", err)
	}

	if err := p.writer.WriteMessages(ctx, kafkago.Message{
		Key:   []byte(event.ConversationID),
		Value: payload,
	}); err != nil {
		return fmt.Errorf("write message to kafka (topic=%s): %w", p.writer.Topic, err)
	}

	return nil
}

func (p *Publisher) Close() error {
	if err := p.writer.Close(); err != nil {
		return fmt.Errorf("close kafka writer: %w", err)
	}
	return nil
}

var _ application.EventPublisher = (*Publisher)(nil)
