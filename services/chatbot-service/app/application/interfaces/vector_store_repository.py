from __future__ import annotations
from abc import ABC, abstractmethod
from uuid import UUID

from app.domain.entities.chunk import Chunk


class VectorStoreRepository(ABC):
    @abstractmethod
    async def add_chunks(self, chunks: list[Chunk]) -> None: ...

    @abstractmethod
    async def similarity_search(self, query_embedding: list[float], top_k: int = 4) -> list[Chunk]: ...

    @abstractmethod
    async def delete_by_document_id(self, document_id: UUID) -> None: ...