package kafka

import (
	"context"
	"encoding/json"
	"fmt"

	kafkago "github.com/segmentio/kafka-go"

	"chat-service/internal/application"
)

type Publisher struct {
	messageSentWriter   *kafkago.Writer
	mediaAttachedWriter *kafkago.Writer
}

func NewPublisher(brokers []string, messageSentTopic, mediaAttachedTopic string) *Publisher {
	return &Publisher{
		messageSentWriter:   newWriter(brokers, messageSentTopic),
		mediaAttachedWriter: newWriter(brokers, mediaAttachedTopic),
	}
}

func newWriter(brokers []string, topic string) *kafkago.Writer {
	return &kafkago.Writer{
		Addr:     kafkago.TCP(brokers...),
		Topic:    topic,
		Balancer: &kafkago.LeastBytes{},
	}
}

func (p *Publisher) PublishMessageSent(ctx context.Context, event application.MessageSentEvent) error {
	payload, err := json.Marshal(event)
	if err != nil {
		return fmt.Errorf("marshal MessageSent event: %w", err)
	}

	if err := p.messageSentWriter.WriteMessages(ctx, kafkago.Message{
		Key:   []byte(event.ConversationID),
		Value: payload,
	}); err != nil {
		return fmt.Errorf("write message to kafka (topic=%s): %w", p.messageSentWriter.Topic, err)
	}

	return nil
}

func (p *Publisher) PublishMediaAttached(ctx context.Context, event application.ChatMediaAttachedEvent) error {
	payload, err := json.Marshal(event)
	if err != nil {
		return fmt.Errorf("marshal ChatMediaAttached event: %w", err)
	}

	if err := p.mediaAttachedWriter.WriteMessages(ctx, kafkago.Message{
		Key:   []byte(event.MessageID),
		Value: payload,
	}); err != nil {
		return fmt.Errorf("write message to kafka (topic=%s): %w", p.mediaAttachedWriter.Topic, err)
	}

	return nil
}

func (p *Publisher) Close() error {
	if err := p.messageSentWriter.Close(); err != nil {
		return fmt.Errorf("close kafka writer (topic=%s): %w", p.messageSentWriter.Topic, err)
	}
	if err := p.mediaAttachedWriter.Close(); err != nil {
		return fmt.Errorf("close kafka writer (topic=%s): %w", p.mediaAttachedWriter.Topic, err)
	}
	return nil
}

var _ application.EventPublisher = (*Publisher)(nil)
