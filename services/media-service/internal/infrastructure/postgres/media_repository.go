package postgres

import (
	"context"
	"database/sql"
	"errors"
	"fmt"

	"media-service/internal/application"
	"media-service/internal/domain"
)

type MediaRepository struct {
	db *sql.DB
}

func NewMediaRepository(db *sql.DB) *MediaRepository {
	return &MediaRepository{db: db}
}

func (r *MediaRepository) CreateAsset(ctx context.Context, a *domain.MediaAsset) error {
	const q = `
		INSERT INTO media_assets
			(id, bucket, object_key, media_type, content_type, size_bytes, width, height,
			 duration_seconds, status, checksum, uploaded_by, created_at)
		VALUES ($1,$2,$3,$4,$5,$6,$7,$8,$9,$10,$11,$12,$13)`
	_, err := r.db.ExecContext(ctx, q,
		a.ID, a.Bucket, a.ObjectKey, a.MediaType, a.ContentType, a.SizeBytes, a.Width, a.Height,
		a.DurationSeconds, a.Status, a.Checksum, a.UploadedBy, a.CreatedAt)
	return err
}

func (r *MediaRepository) GetAssetByID(ctx context.Context, id string) (*domain.MediaAsset, error) {
	const q = `
		SELECT id, bucket, object_key, media_type, content_type, size_bytes, width, height,
		       duration_seconds, status, checksum, uploaded_by, created_at
		FROM media_assets WHERE id = $1`
	return scanAsset(r.db.QueryRowContext(ctx, q, id))
}

func (r *MediaRepository) GetAssetByObjectKey(ctx context.Context, objectKey string) (*domain.MediaAsset, error) {
	const q = `
		SELECT id, bucket, object_key, media_type, content_type, size_bytes, width, height,
		       duration_seconds, status, checksum, uploaded_by, created_at
		FROM media_assets WHERE object_key = $1`
	return scanAsset(r.db.QueryRowContext(ctx, q, objectKey))
}

func (r *MediaRepository) UpdateAsset(ctx context.Context, a *domain.MediaAsset) error {
	const q = `
		UPDATE media_assets SET
			size_bytes=$1, width=$2, height=$3, duration_seconds=$4,
			status=$5, content_type=$6, checksum=$7
		WHERE id=$8`
	res, err := r.db.ExecContext(ctx, q, a.SizeBytes, a.Width, a.Height, a.DurationSeconds,
		a.Status, a.ContentType, a.Checksum, a.ID)
	if err != nil {
		return err
	}
	return checkRowsAffected(res)
}

func scanAsset(row *sql.Row) (*domain.MediaAsset, error) {
	var a domain.MediaAsset
	err := row.Scan(&a.ID, &a.Bucket, &a.ObjectKey, &a.MediaType, &a.ContentType, &a.SizeBytes,
		&a.Width, &a.Height, &a.DurationSeconds, &a.Status, &a.Checksum, &a.UploadedBy, &a.CreatedAt)
	if errors.Is(err, sql.ErrNoRows) {
		return nil, domain.ErrNotFound
	}
	if err != nil {
		return nil, err
	}
	return &a, nil
}

func checkRowsAffected(res sql.Result) error {
	n, err := res.RowsAffected()
	if err != nil {
		return err
	}
	if n == 0 {
		return domain.ErrNotFound
	}
	return nil
}

func (r *MediaRepository) CreateAttachment(ctx context.Context, at *domain.MediaAttachment) error {
	tx, err := r.db.BeginTx(ctx, nil)
	if err != nil {
		return err
	}
	defer func(tx *sql.Tx) {
		err := tx.Rollback()
		if err != nil {

		}
	}(tx)

	if domain.IsUniqueRole(at.Role) {
		const del = `
			DELETE FROM media_attachments
			WHERE owner_service=$1 AND owner_type=$2 AND owner_id=$3 AND role=$4`
		if _, err := tx.ExecContext(ctx, del, at.OwnerService, at.OwnerType, at.OwnerID, at.Role); err != nil {
			return fmt.Errorf("replace existing unique-role attachment: %w", err)
		}
	}

	const ins = `
		INSERT INTO media_attachments
			(id, media_asset_id, owner_service, owner_type, owner_id, role, position, created_at)
		VALUES ($1,$2,$3,$4,$5,$6,$7,$8)`
	if _, err := tx.ExecContext(ctx, ins, at.ID, at.MediaAssetID, at.OwnerService, at.OwnerType,
		at.OwnerID, at.Role, at.Position, at.CreatedAt); err != nil {
		return err
	}

	return tx.Commit()
}

func (r *MediaRepository) GetAttachmentByID(ctx context.Context, id string) (*domain.MediaAttachment, error) {
	const q = `
		SELECT id, media_asset_id, owner_service, owner_type, owner_id, role, position,
		       created_at, updated_at
		FROM media_attachments WHERE id = $1`
	return scanAttachment(r.db.QueryRowContext(ctx, q, id))
}

func (r *MediaRepository) ListAttachments(ctx context.Context, filter application.AttachmentFilter) ([]*domain.MediaAttachment, error) {
	q := `
		SELECT id, media_asset_id, owner_service, owner_type, owner_id, role, position,
		       created_at, updated_at
		FROM media_attachments
		WHERE owner_service=$1 AND owner_type=$2 AND owner_id=$3`
	args := []any{filter.OwnerService, filter.OwnerType, filter.OwnerID}
	if filter.Role != "" {
		q += " AND role=$4"
		args = append(args, filter.Role)
	}
	q += " ORDER BY position ASC, created_at ASC"

	rows, err := r.db.QueryContext(ctx, q, args...)
	if err != nil {
		return nil, err
	}
	defer func(rows *sql.Rows) {
		err := rows.Close()
		if err != nil {

		}
	}(rows)

	var result []*domain.MediaAttachment
	for rows.Next() {
		var a domain.MediaAttachment
		if err := rows.Scan(&a.ID, &a.MediaAssetID, &a.OwnerService, &a.OwnerType, &a.OwnerID,
			&a.Role, &a.Position, &a.CreatedAt, &a.UpdatedAt); err != nil {
			return nil, err
		}
		result = append(result, &a)
	}
	return result, rows.Err()
}

func (r *MediaRepository) DeleteAttachment(ctx context.Context, id string) error {
	const q = `DELETE FROM media_attachments WHERE id=$1`
	res, err := r.db.ExecContext(ctx, q, id)
	if err != nil {
		return err
	}
	return checkRowsAffected(res)
}

func scanAttachment(row *sql.Row) (*domain.MediaAttachment, error) {
	var a domain.MediaAttachment
	err := row.Scan(&a.ID, &a.MediaAssetID, &a.OwnerService, &a.OwnerType, &a.OwnerID,
		&a.Role, &a.Position, &a.CreatedAt, &a.UpdatedAt)
	if errors.Is(err, sql.ErrNoRows) {
		return nil, domain.ErrNotFound
	}
	if err != nil {
		return nil, err
	}
	return &a, nil
}
