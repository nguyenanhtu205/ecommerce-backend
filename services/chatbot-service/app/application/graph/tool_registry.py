from __future__ import annotations
from app.application.interfaces.tool_executor import ToolExecutor


class ToolRegistry:
    def __init__(self) -> None:
        self._tools: list[ToolExecutor] = []

    def register(self, tool: ToolExecutor) -> None:
        self._tools.append(tool)

    async def resolve(self, question: str) -> ToolExecutor | None:
        for tool in self._tools:
            if await tool.can_handle(question):
                return tool
        return None

    def get_default(self) -> ToolExecutor:
        if not self._tools:
            raise RuntimeError("ToolRegistry empty: RAG tool must be registered")
        return self._tools[-1]

    def get_by_name(self, name: str) -> ToolExecutor | None:
        return next((t for t in self._tools if t.name == name), None)