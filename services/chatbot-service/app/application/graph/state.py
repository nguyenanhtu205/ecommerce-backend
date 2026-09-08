from __future__ import annotations
from typing_extensions import TypedDict


class GraphState(TypedDict):
    question: str
    chat_history: list[dict]
    retrieved_chunks: list[dict]
    tool_context: str
    answer: str
    sources: list[dict]
    selected_tool_name: str