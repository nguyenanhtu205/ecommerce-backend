from __future__ import annotations
from pydantic import BaseModel
from uuid import UUID


class UploadResponse(BaseModel):
    document_id: UUID
    filename: str
    status: str


class DocumentStatusResponse(BaseModel):
    document_id: UUID
    filename: str
    status: str
    error_message: str | None = None