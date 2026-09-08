from __future__ import annotations
from dataclasses import dataclass, field


@dataclass
class SourceCitation:
    source: str
    chunk_index: int
    snippet: str
    score: float | None = None


@dataclass
class Answer:
    content: str
    sources: list[SourceCitation] = field(default_factory=list)