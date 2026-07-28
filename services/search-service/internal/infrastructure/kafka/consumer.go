package kafka

import (
	"context"
	"errors"
	"fmt"
	"log"

	kafkago "github.com/segmentio/kafka-go"

	"search-service/internal/application"
)

type Consumer struct {
	reader *kafkago.Reader
	ingest *application.IngestUseCase
}

func NewConsumer(brokers []string, topic string, groupID string, ingest *application.IngestUseCase) *Consumer {
	reader := kafkago.NewReader(kafkago.ReaderConfig{
		Brokers:     brokers,
		Topic:       topic,
		GroupID:     groupID,
		StartOffset: kafkago.FirstOffset,
		MinBytes:    1,
		MaxBytes:    10e6,
	})

	return &Consumer{reader: reader, ingest: ingest}
}

func (c *Consumer) Start(ctx context.Context) error {
	for {
		msg, err := c.reader.FetchMessage(ctx)
		if err != nil {
			if errors.Is(err, context.Canceled) {
				return nil
			}
			return fmt.Errorf("fetch message: %w", err)
		}

		if err := c.ingest.HandleProductListingViewUpdated(ctx, msg.Value); err != nil {
			log.Printf("handle message failed (partition=%d offset=%d key=%s): %v",
				msg.Partition, msg.Offset, string(msg.Key), err)

		}

		if err := c.reader.CommitMessages(ctx, msg); err != nil {
			log.Printf("commit offset failed (partition=%d offset=%d): %v",
				msg.Partition, msg.Offset, err)
		}
	}
}

func (c *Consumer) Close() error {
	return c.reader.Close()
}
