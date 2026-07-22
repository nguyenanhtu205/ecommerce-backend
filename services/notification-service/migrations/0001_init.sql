CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TYPE notification_category AS ENUM ('ORDER', 'PROMOTION', 'SYSTEM');
CREATE TYPE delivery_channel AS ENUM ('EMAIL');
CREATE TYPE delivery_status AS ENUM ('PENDING', 'SENT', 'FAILED');

CREATE TABLE notifications (
                               id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                               user_id uuid NOT NULL,
                               category notification_category NOT NULL,
                               title varchar NOT NULL,
                               content text NOT NULL,
                               image_url varchar,
                               link varchar,
                               is_read boolean NOT NULL DEFAULT false,
                               created_at timestamptz NOT NULL DEFAULT now(),
                               source_event_id varchar NOT NULL UNIQUE
);

CREATE INDEX idx_notifications_user_category_created
    ON notifications (user_id, category, created_at);

CREATE INDEX idx_notifications_user_unread
    ON notifications (user_id, is_read);

CREATE TABLE notification_preferences (
                                          user_id uuid NOT NULL,
                                          category notification_category NOT NULL,
                                          in_app_enabled boolean NOT NULL DEFAULT true,
                                          email_enabled boolean NOT NULL DEFAULT true,
                                          PRIMARY KEY (user_id, category)
);

CREATE TABLE message_deliveries (
                                    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                                    idempotency_key varchar NOT NULL UNIQUE,
                                    source_event_id varchar NOT NULL,
                                    channel delivery_channel NOT NULL,
                                    recipient varchar NOT NULL,
                                    template_code varchar NOT NULL,
                                    payload jsonb NOT NULL,
                                    status delivery_status NOT NULL DEFAULT 'PENDING',
                                    error_message varchar,
                                    sent_at timestamptz,
                                    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX idx_message_deliveries_source_event
    ON message_deliveries (source_event_id);

CREATE TABLE processed_events (
                                  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                                  event_id varchar NOT NULL UNIQUE,
                                  event_type varchar NOT NULL,
                                  processed_at timestamptz NOT NULL DEFAULT now()
);