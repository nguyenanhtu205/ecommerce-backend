from __future__ import annotations
import logging

from google import genai
from google.genai import types as genai_types

from app.application.interfaces.embedding_service import EmbeddingService
from app.domain.exceptions import EmbeddingError

logger = logging.getLogger(__name__)


class GeminiEmbeddingAdapter(EmbeddingService):
    def __init__(
        self,
        api_key: str,
        model_name: str = "gemini-embedding-001",
        output_dimensionality: int = 768,
    ):
        self._client = genai.Client(api_key=api_key)
        self._model_name = model_name.removeprefix("models/")
        self._output_dimensionality = output_dimensionality

    async def embed_documents(self, texts: list[str]) -> list[list[float]]:
        try:
            response = await self._client.aio.models.embed_content(
                model=self._model_name,
                contents=texts,
                config=genai_types.EmbedContentConfig(
                    task_type="RETRIEVAL_DOCUMENT",
                    output_dimensionality=self._output_dimensionality,
                ),
            )
        except Exception as exc:  # noqa: BLE001
            logger.exception("Gemini embedding (documents) failed")
            raise EmbeddingError(str(exc)) from exc

        if response.embeddings is None:
            raise EmbeddingError("Gemini trả về embeddings=None cho embed_documents")

        result: list[list[float]] = []
        for embedding in response.embeddings:
            if embedding.values is None:
                raise EmbeddingError("Gemini trả về 1 embedding rỗng (values=None)")
            result.append(embedding.values)
        return result

    async def embed_query(self, text: str) -> list[float]:
        try:
            response = await self._client.aio.models.embed_content(
                model=self._model_name,
                contents=text,
                config=genai_types.EmbedContentConfig(
                    task_type="RETRIEVAL_QUERY",
                    output_dimensionality=self._output_dimensionality,
                ),
            )
        except Exception as exc:  # noqa: BLE001
            logger.exception("Gemini embedding (query) failed")
            raise EmbeddingError(str(exc)) from exc

        if not response.embeddings or response.embeddings[0].values is None:
            raise EmbeddingError("Gemini trả về embedding rỗng cho embed_query")

        return response.embeddings[0].values