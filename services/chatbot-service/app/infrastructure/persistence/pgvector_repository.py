from __future__ import annotations
import json
from uuid import UUID

import asyncpg

from app.application.interfaces.vector_store_repository import VectorStoreRepository
from app.domain.entities.chunk import Chunk
from app.domain.exceptions import DomainError


def _vector_literal(embedding: list[float]) -> str:
    return "[" + ",".join(str(x) for x in embedding) + "]"


class PgVectorRepository(VectorStoreRepository):
    def __init__(self, pool: asyncpg.Pool):
        self._pool = pool

    async def add_chunks(self, chunks: list[Chunk]) -> None:
        if not chunks:
            return
        if any(c.embedding is None for c in chunks):
            raise DomainError("All chunks must have embeddings before being stored")

        async with self._pool.acquire() as conn:
            async with conn.transaction():
                for c in chunks:
                    await conn.execute(
                        """
                        INSERT INTO chunks (id, document_id, content, chunk_index, source, metadata, embedding)
                        VALUES ($1, $2, $3, $4, $5, $6::jsonb, $7::vector)
                        """,
                        c.id,
                        c.document_id,
                        c.content,
                        c.chunk_index,
                        c.source,
                        json.dumps(c.metadata),
                        _vector_literal(c.embedding),  # type: ignore[arg-type]
                    )

    async def similarity_search(self, query_embedding: list[float], top_k: int = 4) -> list[Chunk]:
        query_vec = _vector_literal(query_embedding)
        async with self._pool.acquire() as conn:
            rows = await conn.fetch(
                """
                SELECT id, document_id, content, chunk_index, source, metadata,
                       1 - (embedding <=> $1::vector) AS score
                FROM chunks
                ORDER BY embedding <=> $1::vector
                LIMIT $2
                """,
                query_vec,
                top_k,
            )

        return [
            Chunk(
                id=row["id"],
                document_id=row["document_id"],
                content=row["content"],
                chunk_index=row["chunk_index"],
                source=row["source"],
                metadata=json.loads(row["metadata"]) if row["metadata"] else {},
                score=float(row["score"]),
            )
            for row in rows
        ]

    async def delete_by_document_id(self, document_id: UUID) -> None:
        async with self._pool.acquire() as conn:
            await conn.execute("DELETE FROM chunks WHERE document_id = $1", document_id)