from __future__ import annotations
from alembic import op

revision = "0001_init"
down_revision = None
branch_labels = None
depends_on = None

EMBEDDING_DIM = 768


def upgrade() -> None:
    op.execute("CREATE EXTENSION IF NOT EXISTS vector;")

    op.execute(
        f"""
        CREATE TABLE IF NOT EXISTS chunks (
            id UUID PRIMARY KEY,
            document_id UUID NOT NULL,
            content TEXT NOT NULL,
            chunk_index INTEGER NOT NULL,
            source TEXT NOT NULL,
            metadata JSONB DEFAULT '{{}}'::jsonb,
            embedding VECTOR({EMBEDDING_DIM}) NOT NULL,
            created_at TIMESTAMPTZ DEFAULT now()
        );
        """
    )

    op.execute(
        """
        CREATE INDEX IF NOT EXISTS chunks_embedding_idx
        ON chunks USING hnsw (embedding vector_cosine_ops);
        """
    )

    op.execute("CREATE INDEX IF NOT EXISTS chunks_document_id_idx ON chunks (document_id);")


def downgrade() -> None:
    op.execute("DROP INDEX IF EXISTS chunks_document_id_idx;")
    op.execute("DROP INDEX IF EXISTS chunks_embedding_idx;")
    op.execute("DROP TABLE IF EXISTS chunks;")