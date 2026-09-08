from __future__ import annotations
from pathlib import Path
from uuid import UUID

from fastapi import APIRouter, Depends, UploadFile, File, BackgroundTasks, HTTPException

from app.presentation.api.schemas.ingest_schemas import UploadResponse, DocumentStatusResponse
from app.presentation.api.dependencies import get_ingest_use_case
from app.application.use_cases.ingest_document import IngestDocumentUseCase
from app.infrastructure.ingestion.document_loader_factory import extract_text
from app.domain.exceptions import UnsupportedFileTypeError

router = APIRouter(prefix="/documents", tags=["ingest"])

_EXTENSION_CONTENT_TYPES = {
    ".md": "text/markdown",
    ".markdown": "text/markdown",
    ".txt": "text/plain",
    ".pdf": "application/pdf",
}


def _resolve_content_type(filename: str, provided_content_type: str | None) -> str:
    if provided_content_type and provided_content_type != "application/octet-stream":
        return provided_content_type
    ext = Path(filename).suffix.lower()
    return _EXTENSION_CONTENT_TYPES.get(ext, "text/plain")


@router.post("/upload", response_model=UploadResponse)
async def upload_document(
    background_tasks: BackgroundTasks,
    file: UploadFile = File(...),
    use_case: IngestDocumentUseCase = Depends(get_ingest_use_case),
):
    file_bytes = await file.read()
    filename = file.filename
    if not filename:
        raise HTTPException(
            status_code=400,
            detail="Filename is required",
        )
    content_type = _resolve_content_type(filename, file.content_type)

    document = await use_case.register_pending(
        filename=filename,
        content_type=content_type,
        file_bytes=file_bytes,
    )

    try:
        raw_text = extract_text(file_bytes, content_type)
    except UnsupportedFileTypeError as exc:
        raise HTTPException(status_code=400, detail=str(exc)) from exc

    background_tasks.add_task(use_case.run, document.id, raw_text)

    return UploadResponse(document_id=document.id, filename=document.filename, status=document.status)


@router.get("/{document_id}/status", response_model=DocumentStatusResponse)
async def get_document_status(
    document_id: UUID,
    use_case: IngestDocumentUseCase = Depends(get_ingest_use_case),
):
    document = use_case.get_status(document_id)
    if document is None:
        raise HTTPException(status_code=404, detail="Document not found")

    return DocumentStatusResponse(
        document_id=document.id,
        filename=document.filename,
        status=document.status,
        error_message=document.error_message,
    )