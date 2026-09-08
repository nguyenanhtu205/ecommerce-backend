from __future__ import annotations
from pydantic import BaseModel, Field


class AskRequest(BaseModel):
    question: str = Field(..., min_length=1)
    session_id: str = Field(..., min_length=1)


class SourceSchema(BaseModel):
    source: str
    chunk_index: int
    snippet: str
    score: float | None = None


class AskResponse(BaseModel):
    answer: str
    sources: list[SourceSchema]