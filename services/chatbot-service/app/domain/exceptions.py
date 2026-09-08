class DomainError(Exception):
    pass


class DocumentNotFoundError(DomainError):
    def __init__(self, document_id: str):
        super().__init__(f"Document not found: {document_id}")


class UnsupportedFileTypeError(DomainError):
    def __init__(self, content_type: str):
        super().__init__(f"Unsupported file type: {content_type}")


class EmbeddingError(DomainError):
    pass


class LLMGenerationError(DomainError):
    pass