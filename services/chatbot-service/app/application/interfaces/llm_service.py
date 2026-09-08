from __future__ import annotations
from abc import ABC, abstractmethod


class LLMService(ABC):
    @abstractmethod
    async def generate_answer(self, question: str, context: str, chat_history: list[dict] | None = None) -> str: ...