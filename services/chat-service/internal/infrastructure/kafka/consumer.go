package kafka

import (
	"context"
	"errors"
	"fmt"
	"log"

	kafkago "github.com/segmentio/kafka-go"
)

type HandlerFunc func(ctx context.Context, payload []byte) error

type Consumer struct {
	reader  *kafkago.Reader
	handler HandlerFunc
}

func NewConsumer(brokers []string, topic, groupID string, handler HandlerFunc) *Consumer {
	reader := kafkago.NewReader(kafkago.ReaderConfig{
		Brokers:     brokers,
		Topic:       topic,
		GroupID:     groupID,
		StartOffset: kafkago.FirstOffset,
		MinBytes:    1,
		MaxBytes:    10e6,
	})

	return &Consumer{reader: reader, handler: handler}
}

func (c *Consumer) Start(ctx context.Context) error {
	for {
		msg, err := c.reader.FetchMessage(ctx)
		if err != nil {
			if errors.Is(err, context.Canceled) {
				return nil
			}
			return fmt.Errorf("fetch message (topic=%s): %w", c.reader.Config().Topic, err)
		}

		if err := c.handler(ctx, msg.Value); err != nil {
			log.Printf("handle message failed (topic=%s partition=%d offset=%d key=%s): %v",
				c.reader.Config().Topic, msg.Partition, msg.Offset, string(msg.Key), err)
		}

		if err := c.reader.CommitMessages(ctx, msg); err != nil {
			log.Printf("commit offset failed (topic=%s partition=%d offset=%d): %v",
				c.reader.Config().Topic, msg.Partition, msg.Offset, err)
		}
	}
}

func (c *Consumer) Close() error {
	return c.reader.Close()
}
