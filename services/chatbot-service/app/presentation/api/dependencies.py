from __future__ import annotations
from functools import lru_cache

import asyncpg
from fastapi import Request
from fastapi import Header, HTTPException
from uuid import UUID

from app.infrastructure.config.settings import Settings, get_settings
from app.infrastructure.persistence.pgvector_repository import PgVectorRepository
from app.infrastructure.llm.gemini_llm_adapter import GeminiLLMAdapter
from app.infrastructure.llm.gemini_embedding_adapter import GeminiEmbeddingAdapter
from app.infrastructure.storage.minio_adapter import MinioStorageAdapter
from app.infrastructure.cache.redis_cache_adapter import RedisCacheAdapter

from app.application.use_cases.ingest_document import IngestDocumentUseCase, DocumentRegistry
from app.application.use_cases.ask_question import AskQuestionUseCase
from app.application.graph.tool_registry import ToolRegistry
from app.application.graph.rag_tool import RagTool
from app.application.graph.graph_builder import build_graph


@lru_cache
def get_cached_settings() -> Settings:
    return get_settings()


def get_settings_dep() -> Settings:
    return get_cached_settings()


def get_db_pool(request: Request) -> asyncpg.Pool:
    return request.app.state.db_pool


def get_vector_store(request: Request) -> PgVectorRepository:
    return request.app.state.vector_store


def get_embedding_service(request: Request) -> GeminiEmbeddingAdapter:
    return request.app.state.embedding_service


def get_llm_service(request: Request) -> GeminiLLMAdapter:
    return request.app.state.llm_service


def get_file_storage(request: Request) -> MinioStorageAdapter:
    return request.app.state.file_storage


def get_cache_service(request: Request) -> RedisCacheAdapter:
    return request.app.state.cache_service


def get_document_registry(request: Request) -> DocumentRegistry:
    return request.app.state.document_registry


def get_tool_registry(request: Request) -> ToolRegistry:
    return request.app.state.tool_registry


def get_ingest_use_case(request: Request) -> IngestDocumentUseCase:
    return IngestDocumentUseCase(
        file_storage=get_file_storage(request),
        embedding_service=get_embedding_service(request),
        vector_store=get_vector_store(request),
        document_registry=get_document_registry(request),
        chunk_size=get_cached_settings().chunk_size,
        chunk_overlap=get_cached_settings().chunk_overlap,
    )


def get_ask_question_use_case(request: Request) -> AskQuestionUseCase:
    return AskQuestionUseCase(
        graph=request.app.state.compiled_graph,
        cache_service=get_cache_service(request),
        cache_ttl=get_cached_settings().answer_cache_ttl_seconds,
    )


def build_app_state_factory():
    async def _build(settings: Settings, db_pool: asyncpg.Pool):
        vector_store = PgVectorRepository(db_pool)
        embedding_service = GeminiEmbeddingAdapter(
            api_key=settings.gemini_api_key,
            model_name=settings.gemini_embedding_model,
            output_dimensionality=settings.gemini_embedding_dim,
        )
        llm_service = GeminiLLMAdapter(
            api_key=settings.gemini_api_key, model_name=settings.gemini_llm_model
        )
        file_storage = MinioStorageAdapter(
            endpoint=settings.minio_endpoint,
            access_key=settings.minio_access_key,
            secret_key=settings.minio_secret_key,
            bucket=settings.minio_bucket,
            secure=settings.minio_secure,
        )
        cache_service = RedisCacheAdapter(
            redis_url=settings.redis_url,
            history_ttl_seconds=settings.chat_history_ttl_seconds,
        )
        document_registry = DocumentRegistry()

        tool_registry = ToolRegistry()
        tool_registry.register(RagTool())

        compiled_graph = build_graph(
            embedding_service=embedding_service,
            vector_store=vector_store,
            llm_service=llm_service,
            tool_registry=tool_registry,
            top_k=settings.retrieval_top_k,
        )

        return {
            "vector_store": vector_store,
            "embedding_service": embedding_service,
            "llm_service": llm_service,
            "file_storage": file_storage,
            "cache_service": cache_service,
            "document_registry": document_registry,
            "tool_registry": tool_registry,
            "compiled_graph": compiled_graph,
        }

    return _build

def get_current_user_id(x_user_id: str | None = Header(default=None, alias="X-User-Id")) -> UUID:
    if not x_user_id:
        raise HTTPException(status_code=401, detail="Missing X-User-Id header")
    try:
        return UUID(x_user_id)
    except ValueError:
        raise HTTPException(status_code=401, detail="Invalid X-User-Id header") from None