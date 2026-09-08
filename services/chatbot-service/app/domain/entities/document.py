from __future__ import annotations
from dataclasses import dataclass, field
from datetime import datetime, UTC
from enum import Enum
from uuid import UUID, uuid4


class DocumentStatus(str, Enum):
    PENDING = "pending"
    PROCESSING = "processing"
    INDEXED = "indexed"
    FAILED = "failed"


@dataclass
class Document:
    filename: str
    content_type: str
    storage_path: str
    id: UUID = field(default_factory=uuid4)
    status: DocumentStatus = DocumentStatus.PENDING
    error_message: str | None = None
    created_at: datetime = field(default_factory=lambda: datetime.now(UTC))
    updated_at: datetime = field(default_factory=lambda: datetime.now(UTC))

    def mark_processing(self) -> None:
        self.status = DocumentStatus.PROCESSING
        self.updated_at = datetime.now(UTC)

    def mark_indexed(self) -> None:
        self.status = DocumentStatus.INDEXED
        self.updated_at = datetime.now(UTC)

    def mark_failed(self, error_message: str) -> None:
        self.status = DocumentStatus.FAILED
        self.error_message = error_message
        self.updated_at = datetime.now(UTC)