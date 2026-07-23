-- +goose Up
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

CREATE TYPE media_type AS ENUM ('IMAGE', 'VIDEO');
CREATE TYPE media_status AS ENUM ('PENDING_UPLOAD', 'PROCESSING', 'READY', 'FAILED', 'DELETING', 'DELETED');

CREATE TABLE media_assets (
    id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    bucket           VARCHAR NOT NULL,
    object_key       VARCHAR NOT NULL UNIQUE,
    media_type       media_type NOT NULL,
    content_type     VARCHAR NOT NULL,
    size_bytes       BIGINT,
    width            INT,
    height           INT,
    duration_seconds INT,
    status           media_status NOT NULL DEFAULT 'PENDING_UPLOAD',
    checksum         VARCHAR,
    uploaded_by      UUID NOT NULL,
    created_at       TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE media_attachments (
    id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    media_asset_id UUID NOT NULL REFERENCES media_assets(id),
    owner_service  VARCHAR NOT NULL,
    owner_type     VARCHAR NOT NULL,
    owner_id       UUID NOT NULL,
    role           VARCHAR NOT NULL,
    position       INT NOT NULL DEFAULT 0,
    created_at     TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at     TIMESTAMPTZ,
    deleted_at     TIMESTAMPTZ
);

CREATE INDEX idx_attachments_owner ON media_attachments (owner_service, owner_type, owner_id);
CREATE INDEX idx_attachments_owner_role ON media_attachments (owner_service, owner_type, owner_id, role);
CREATE INDEX idx_attachments_asset ON media_attachments (media_asset_id);

CREATE UNIQUE INDEX uq_attachments_asset_owner_role
    ON media_attachments (media_asset_id, owner_service, owner_type, owner_id, role)
    WHERE deleted_at IS NULL;

CREATE UNIQUE INDEX uq_attachments_owner_unique_role
    ON media_attachments (owner_service, owner_type, owner_id, role)
    WHERE role IN ('avatar', 'thumbnail', 'cover') AND deleted_at IS NULL;

-- +goose Down
DROP TABLE IF EXISTS media_attachments;
DROP TABLE IF EXISTS media_assets;
DROP TYPE IF EXISTS media_status;
DROP TYPE IF EXISTS media_type;
