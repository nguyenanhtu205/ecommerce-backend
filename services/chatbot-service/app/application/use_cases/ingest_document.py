from __future__ import annotations
import logging
from uuid import UUID

from langchain_text_splitters import RecursiveCharacterTextSplitter

from app.domain.entities.chunk import Chunk
from app.domain.entities.document import Document
from app.application.interfaces.vector_store_repository import VectorStoreRepository
from app.application.interfaces.embedding_service import EmbeddingService
from app.application.interfaces.file_storage_service import FileStorageService

logger = logging.getLogger(__name__)


class DocumentRegistry:
    def __init__(self) -> None:
        self._store: dict[UUID, Document] = {}

    def save(self, document: Document) -> None:
        self._store[document.id] = document

    def get(self, document_id: UUID) -> Document | None:
        return self._store.get(document_id)


class IngestDocumentUseCase:
    def __init__(
        self,
        file_storage: FileStorageService,
        embedding_service: EmbeddingService,
        vector_store: VectorStoreRepository,
        document_registry: DocumentRegistry,
        chunk_size: int = 1000,
        chunk_overlap: int = 150,
    ) -> None:
        self._file_storage = file_storage
        self._embedding_service = embedding_service
        self._vector_store = vector_store
        self._registry = document_registry
        self._splitter = RecursiveCharacterTextSplitter(
            chunk_size=chunk_size, chunk_overlap=chunk_overlap
        )

    async def register_pending(self, filename: str, content_type: str, file_bytes: bytes) -> Document:
        object_name = f"{filename}"
        storage_path = await self._file_storage.upload(
            file_bytes=file_bytes, object_name=object_name, content_type=content_type
        )
        document = Document(
            filename=filename, content_type=content_type, storage_path=storage_path
        )
        self._registry.save(document)
        return document

    async def run(self, document_id: UUID, raw_text: str) -> None:
        document = self._registry.get(document_id)
        if document is None:
            logger.error("Document %s not found in registry", document_id)
            return

        try:
            document.mark_processing()
            self._registry.save(document)

            texts = self._splitter.split_text(raw_text)
            if not texts:
                raise ValueError("Document produced 0 chunks after splitting")

            embeddings = await self._embedding_service.embed_documents(texts)

            chunks = [
                Chunk(
                    document_id=document.id,
                    content=text,
                    chunk_index=idx,
                    source=document.filename,
                    embedding=embedding,
                    metadata={"content_type": document.content_type},
                )
                for idx, (text, embedding) in enumerate(zip(texts, embeddings))
            ]

            await self._vector_store.add_chunks(chunks)

            document.mark_indexed()
            self._registry.save(document)
            logger.info("Document %s indexed with %d chunks", document.id, len(chunks))

        except Exception as exc:  # noqa: BLE001
            logger.exception("Failed to ingest document %s", document_id)
            document.mark_failed(str(exc))
            self._registry.save(document)

    def get_status(self, document_id: UUID) -> Document | None:
        return self._registry.get(document_id)