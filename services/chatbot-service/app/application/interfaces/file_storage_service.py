from __future__ import annotations
from abc import ABC, abstractmethod


class FileStorageService(ABC):
    @abstractmethod
    async def upload(self, file_bytes: bytes, object_name: str, content_type: str) -> str: ...

    @abstractmethod
    async def download(self, storage_path: str) -> bytes: ...