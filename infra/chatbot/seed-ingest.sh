#!/bin/sh
set -e

CHATBOT_URL="${CHATBOT_SERVICE_URL:-http://chatbot-service:8080}"
SEED_FILE="${SEED_FILE:-/seed/user-guide.md}"
SOURCE_NAME="${SEED_SOURCE_NAME:-user-guide.md}"

echo "[chatbot-seed] Waiting for chatbot-service to be ready..."
until curl -sf "${CHATBOT_URL}/health" > /dev/null 2>&1; do
  echo "[chatbot-seed] Waiting for chatbot-service..."
  sleep 2
done

CURRENT_HASH=$(sha256sum "$SEED_FILE" | awk '{print $1}')
STORED_HASH=$(psql "$POSTGRES_DSN" -tAc \
  "SELECT metadata->>'content_hash' FROM chunks WHERE source = '${SOURCE_NAME}' LIMIT 1;" 2>/dev/null || echo "")

if [ "$CURRENT_HASH" = "$STORED_HASH" ] && [ -n "$STORED_HASH" ]; then
  echo "[chatbot-seed] Skipping: '${SOURCE_NAME}' has not changed (hash=${CURRENT_HASH})."
  exit 0
fi

if [ -n "$STORED_HASH" ]; then
  echo "[chatbot-seed] '${SOURCE_NAME}' has changed. Deleting old chunks before re-ingesting..."
  psql "$POSTGRES_DSN" -c "DELETE FROM chunks WHERE source = '${SOURCE_NAME}';"
fi

echo "[chatbot-seed] Uploading ${SEED_FILE}..."
RESPONSE=$(curl -sf -F "file=@${SEED_FILE};type=text/markdown;filename=${SOURCE_NAME}" "${CHATBOT_URL}/documents/upload")
DOCUMENT_ID=$(echo "$RESPONSE" | sed -n 's/.*"document_id":"\([^"]*\)".*/\1/p')

if [ -z "$DOCUMENT_ID" ]; then
  echo "[chatbot-seed] Upload failed: ${RESPONSE}"
  exit 1
fi

echo "[chatbot-seed] document_id=${DOCUMENT_ID}. Waiting for ingestion to complete..."
for i in $(seq 1 30); do
  STATUS_RESPONSE=$(curl -sf "${CHATBOT_URL}/documents/${DOCUMENT_ID}/status")
  STATUS=$(echo "$STATUS_RESPONSE" | sed -n 's/.*"status":"\([^"]*\)".*/\1/p')
  echo "[chatbot-seed] status=${STATUS}"

  if [ "$STATUS" = "indexed" ]; then
    echo "[chatbot-seed] Ingestion completed. Storing content_hash in metadata..."
    psql "$POSTGRES_DSN" -c \
      "UPDATE chunks SET metadata = metadata || jsonb_build_object('content_hash', '${CURRENT_HASH}') WHERE source = '${SOURCE_NAME}';"
    echo "[chatbot-seed] Completed successfully."
    exit 0
  fi

  if [ "$STATUS" = "failed" ]; then
    echo "[chatbot-seed] Ingestion failed: ${STATUS_RESPONSE}"
    exit 1
  fi

  sleep 2
done

echo "[chatbot-seed] Timeout while waiting for ingestion to complete."
exit 1