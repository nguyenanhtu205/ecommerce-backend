from __future__ import annotations
from app.application.interfaces.tool_executor import ToolExecutor
from app.application.graph.nodes import RAG_TOOL_NAME


class RagTool(ToolExecutor):
    name = RAG_TOOL_NAME

    async def can_handle(self, question: str) -> bool:
        return True

    async def execute(self, question: str, context: dict) -> str:
        return ""