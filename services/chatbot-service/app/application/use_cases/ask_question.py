from __future__ import annotations
import hashlib
import json
import logging

from langgraph.graph.state import CompiledStateGraph

from app.domain.entities.answer import Answer, SourceCitation
from app.application.interfaces.cache_service import CacheService

logger = logging.getLogger(__name__)


class AskQuestionUseCase:
    def __init__(self, graph: CompiledStateGraph, cache_service: CacheService, cache_ttl: int = 3600):
        self._graph = graph
        self._cache = cache_service
        self._cache_ttl = cache_ttl

    @staticmethod
    def _cache_key(question: str) -> str:
        digest = hashlib.sha256(question.strip().lower().encode("utf-8")).hexdigest()
        return f"answer_cache:{digest}"

    async def execute(self, question: str, session_id: str) -> Answer:
        cache_key = self._cache_key(question)
        cached = await self._cache.get(cache_key)
        if cached:
            logger.info("Cache hit for question hash=%s", cache_key)
            data = json.loads(cached) if isinstance(cached, str) else cached
            return Answer(
                content=data["content"],
                sources=[SourceCitation(**s) for s in data["sources"]],
            )

        history = await self._cache.get_history(session_id)

        result_state = await self._graph.ainvoke(
            {
                "question": question,
                "chat_history": history,
                "retrieved_chunks": [],
                "tool_context": "",
                "answer": "",
                "sources": [],
                "selected_tool_name": "",
            }
        )

        answer = Answer(
            content=result_state["answer"],
            sources=[SourceCitation(**s) for s in result_state["sources"]],
        )

        await self._cache.append_history(session_id, "user", question)
        await self._cache.append_history(session_id, "assistant", answer.content)

        await self._cache.set(
            cache_key,
            json.dumps(
                {
                    "content": answer.content,
                    "sources": [s.__dict__ for s in answer.sources],
                }
            ),
            ttl_seconds=self._cache_ttl,
        )

        return answer