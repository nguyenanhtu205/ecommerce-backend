from __future__ import annotations
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", env_file_encoding="utf-8", extra="ignore")

    app_name: str
    log_level: str

    gemini_api_key: str
    gemini_llm_model: str
    gemini_embedding_model: str
    gemini_embedding_dim: int

    postgres_dsn: str

    minio_endpoint: str
    minio_access_key: str
    minio_secret_key: str
    minio_bucket: str
    minio_secure: bool

    redis_url: str
    answer_cache_ttl_seconds: int
    chat_history_ttl_seconds: int

    chunk_size: int
    chunk_overlap: int
    retrieval_top_k: int


def get_settings() -> Settings:
    return Settings()  # type: ignore[call-arg]