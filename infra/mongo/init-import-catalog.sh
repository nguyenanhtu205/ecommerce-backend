#!/bin/bash
set -e

DB_NAME="product_catalog_db"
DATA_DIR="/docker-entrypoint-initdb.d/data"

import_if_empty() {
  local collection="$1"
  local file="$2"
  local count

  count=$(mongosh --quiet "$DB_NAME" --eval "db.getCollection('${collection}').countDocuments({})")

  if [ "$count" -gt 0 ]; then
    echo "[init-import-catalog] Skipping '${collection}': it already contains ${count} documents."
    return
  fi

  mongoimport \
    --db "$DB_NAME" \
    --collection "$collection" \
    --jsonArray \
    --mode=upsert \
    --upsertFields=_id \
    --file "$file"

  echo "[init-import-catalog] Successfully imported '${collection}' from ${file}."
}

import_if_empty "categories" "${DATA_DIR}/product_catalog_db_categories.json"
import_if_empty "category_attributes" "${DATA_DIR}/product_catalog_db_category_attributes.json"