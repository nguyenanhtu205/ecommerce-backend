from __future__ import annotations
import io

from pypdf import PdfReader

from app.domain.exceptions import UnsupportedFileTypeError

SUPPORTED_TYPES = {"text/plain", "text/markdown", "application/pdf"}


def extract_text(file_bytes: bytes, content_type: str) -> str:
    if content_type in {"text/plain", "text/markdown"}:
        return file_bytes.decode("utf-8", errors="ignore")

    if content_type == "application/pdf":
        reader = PdfReader(io.BytesIO(file_bytes))
        return "\n".join(page.extract_text() or "" for page in reader.pages)

    raise UnsupportedFileTypeError(content_type)