from __future__ import annotations
from abc import ABC, abstractmethod
from typing import Any


class CacheService(ABC):
    @abstractmethod
    async def get(self, key: str) -> Any | None: ...

    @abstractmethod
    async def set(self, key: str, value: Any, ttl_seconds: int | None = None) -> None: ...

    @abstractmethod
    async def append_history(self, session_id: str, role: str, content: str) -> None: ...

    @abstractmethod
    async def get_history(self, session_id: str) -> list[dict]: ...