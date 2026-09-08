from __future__ import annotations
from uuid import UUID
from pydantic import BaseModel, Field


class AskRequest(BaseModel):
    question: str = Field(..., min_length=1)


class SourceSchema(BaseModel):
    source: str
    chunk_index: int
    snippet: str
    score: float | None = None


class AskResponse(BaseModel):
    answer: str
    sources: list[SourceSchema]


class ChatMessageSchema(BaseModel):
    role: str
    content: str


class ChatHistoryResponse(BaseModel):
    user_id: UUID
    messages: list[ChatMessageSchema]