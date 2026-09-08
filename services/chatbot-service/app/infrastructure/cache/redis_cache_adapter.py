from __future__ import annotations
import json
from typing import Any

import redis.asyncio as redis

from app.application.interfaces.cache_service import CacheService


class RedisCacheAdapter(CacheService):
    def __init__(self, redis_url: str, history_ttl_seconds: int = 3600):
        self._client = redis.from_url(redis_url, decode_responses=True)
        self._history_ttl = history_ttl_seconds

    async def get(self, key: str) -> Any | None:
        return await self._client.get(key)

    async def set(self, key: str, value: Any, ttl_seconds: int | None = None) -> None:
        await self._client.set(key, value, ex=ttl_seconds)

    async def append_history(self, session_id: str, role: str, content: str) -> None:
        key = f"chat_history:{session_id}"
        await self._client.rpush(key, json.dumps({"role": role, "content": content}))
        await self._client.expire(key, self._history_ttl)

    async def get_history(self, session_id: str) -> list[dict]:
        key = f"chat_history:{session_id}"
        raw_items = await self._client.lrange(key, 0, -1)
        return [json.loads(item) for item in raw_items]