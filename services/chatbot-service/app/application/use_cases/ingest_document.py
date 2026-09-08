from __future__ import annotations
import logging
from uuid import UUID

from langchain_text_splitters import RecursiveCharacterTextSplitter

from app.domain.entities.chunk import Chunk
from app.domain.entities.document import Document
from app.application.interfaces.vector_store_repository import VectorStoreRepository
from app.application.interfaces.embedding_service import EmbeddingService
from app.application.interfaces.file_storage_service import FileStorageService
from app.infrastructure.ingestion.markdown_section_splitter import split_markdown_sections

logger = logging.getLogger(__name__)

MARKDOWN_CONTENT_TYPES = {"text/markdown", "text/x-markdown"}


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
        self._chunk_size = chunk_size
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

    def _build_chunk_texts(self, raw_text: str, content_type: str) -> tuple[list[str], list[str]]:
        if content_type not in MARKDOWN_CONTENT_TYPES:
            texts = self._splitter.split_text(raw_text)
            return texts, [""] * len(texts)

        sections = split_markdown_sections(raw_text)
        texts: list[str] = []
        headings: list[str] = []

        for section in sections:
            heading = section["heading"]
            body = section["content"].strip()
            if not body:
                continue

            prefix = f"{heading}\n" if heading else ""
            full_text = f"{prefix}{body}"

            if len(full_text) <= self._chunk_size:
                texts.append(full_text)
                headings.append(heading)
                continue

            for sub in self._splitter.split_text(body):
                texts.append(f"{prefix}{sub}" if prefix else sub)
                headings.append(heading)

        return texts, headings

    async def run(self, document_id: UUID, raw_text: str) -> None:
        document = self._registry.get(document_id)
        if document is None:
            logger.error("Document %s not found in registry", document_id)
            return

        try:
            document.mark_processing()
            self._registry.save(document)

            texts, headings = self._build_chunk_texts(raw_text, document.content_type)
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
                    metadata={
                        "content_type": document.content_type,
                        **({"heading": heading} if heading else {}),
                    },
                )
                for idx, (text, heading, embedding) in enumerate(
                    zip(texts, headings, embeddings)
                )
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