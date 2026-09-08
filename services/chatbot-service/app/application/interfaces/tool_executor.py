from __future__ import annotations
from abc import ABC, abstractmethod
from typing import Any


class ToolExecutor(ABC):
    name: str

    @abstractmethod
    async def can_handle(self, question: str) -> bool: ...

    @abstractmethod
    async def execute(self, question: str, context: dict[str, Any]) -> str: ...