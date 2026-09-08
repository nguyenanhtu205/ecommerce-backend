from __future__ import annotations
import asyncio
import io
import logging

from minio import Minio

from app.application.interfaces.file_storage_service import FileStorageService

logger = logging.getLogger(__name__)


class MinioStorageAdapter(FileStorageService):
    def __init__(
        self,
        endpoint: str,
        access_key: str,
        secret_key: str,
        bucket: str,
        secure: bool = False,
    ):
        self._client = Minio(endpoint, access_key=access_key, secret_key=secret_key, secure=secure)
        self._bucket = bucket
        self._ensure_bucket()

    def _ensure_bucket(self) -> None:
        if not self._client.bucket_exists(self._bucket):
            self._client.make_bucket(self._bucket)
            logger.info("Created MinIO bucket: %s", self._bucket)

    async def upload(self, file_bytes: bytes, object_name: str, content_type: str) -> str:
        def _put() -> None:
            self._client.put_object(
                self._bucket,
                object_name,
                data=io.BytesIO(file_bytes),
                length=len(file_bytes),
                content_type=content_type,
            )

        await asyncio.to_thread(_put)
        return f"{self._bucket}/{object_name}"

    async def download(self, storage_path: str) -> bytes:
        _, object_name = storage_path.split("/", 1)

        def _get() -> bytes:
            response = self._client.get_object(self._bucket, object_name)
            try:
                return response.read()
            finally:
                response.close()
                response.release_conn()

        return await asyncio.to_thread(_get)