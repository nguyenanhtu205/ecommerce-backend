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
create_topic user.pickup-address-snapshot-updated.v1
create_topic seller.shop-created.v1

# --- product ---
create_topic product-catalog.product-created.v1
create_topic product-catalog.product-listing-view-updated.v1
create_topic product-catalog.product-media-attached.v1

# --- notification ---
create_topic notification.otp-requested.v1
create_topic notification.shop-activated.v1

# --- order ---
create_topic order.payment-succeeded.v1
create_topic order.payment-failed.v1
create_topic order.stock-reserved.v1
create_topic order.stock-reservation-failed.v1
create_topic order.cancel-order.v1

# --- checkout ---
create_topic checkout.initiated.v1
create_topic checkout.reserve-order-stock.v1

# --- promotion ---
create_topic promotion.redeem-voucher.v1
create_topic promotion.release-voucher.v1
create_topic promotion.voucher-redeemed.v1
create_topic promotion.voucher-redemption-failed.v1

# --- payment ---
create_topic payment.create-payment.v1
create_topic payment.vnpay-confirmed.v1
create_topic payment.redirect-created.v1

# --- inventory ---
create_topic inventory.release-stock.v1
create_topic inventory.reserve-stock.v1
create_topic inventory.stock-reserved.v1
create_topic inventory.stock-reservation-failed.v1
create_topic inventory.commit-stock.v1

# --- shipping ---
create_topic shipping.create-shipment.v1
create_topic shipping.shipment-created.v1
create_topic shipping.shipment-creation-failed.v1
create_topic shipping.order-delivered.v1

# --- chat ---
create_topic chat.message-sent.v1

# --- review ---
create_topic review.review-aggregate-updated.v1

echo "=== All topics created ==="

echo "--- Topic list ---"
"$KAFKA_BIN" --bootstrap-server "$BOOTSTRAP" --list