#!/bin/bash
set -e

BOOTSTRAP="kafka:9092"
KAFKA_BIN="/opt/kafka/bin/kafka-topics.sh"

create_topic() {
  local topic=$1
  local partitions=${2:-1}
  local replication=${3:-1}

  echo "→ Creating topic: $topic (partitions=$partitions, replication=$replication)"
  "$KAFKA_BIN" --bootstrap-server "$BOOTSTRAP" \
    --create --if-not-exists \
    --topic "$topic" \
    --partitions "$partitions" \
    --replication-factor "$replication"
}

echo "=== Creating Kafka topics ==="

# --- user / auth ---
create_topic user.registered.v1

# --- product ---
create_topic product-catalog.product-created.v1
create_topic product-catalog.product-listing-view-updated.v1

# --- notification ---
create_topic notification.otp-requested.v1
create_topic notification.shop-activated.v1

echo "=== All topics created ==="

echo "--- Topic list ---"
"$KAFKA_BIN" --bootstrap-server "$BOOTSTRAP" --list