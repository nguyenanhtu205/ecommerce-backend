from __future__ import annotations
from dataclasses import dataclass, field
from uuid import UUID, uuid4


@dataclass
class Chunk:
    document_id: UUID
    content: str
    chunk_index: int
    source: str
    id: UUID = field(default_factory=uuid4)
    embedding: list[float] | None = None
    metadata: dict = field(default_factory=dict)
    score: float | None = None