#!/bin/bash
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "notification_db" \
  -f /docker-entrypoint-initdb.d/notification/0001_init.sql