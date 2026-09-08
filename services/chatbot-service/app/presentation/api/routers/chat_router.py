from __future__ import annotations
import logging
from uuid import UUID

from fastapi import APIRouter, Depends, HTTPException

from app.presentation.api.schemas.chat_schemas import (
    AskRequest,
    AskResponse,
    SourceSchema,
    ChatHistoryResponse,
    ChatMessageSchema,
)
from app.presentation.api.dependencies import (
    get_ask_question_use_case,
    get_current_user_id,
    get_cache_service,
)
from app.application.use_cases.ask_question import AskQuestionUseCase
from app.application.interfaces.cache_service import CacheService
from app.domain.exceptions import DomainError

logger = logging.getLogger(__name__)
router = APIRouter(prefix="/chatbot/chat", tags=["chat"])


@router.post("/ask", response_model=AskResponse)
async def ask_question(
    payload: AskRequest,
    user_id: UUID = Depends(get_current_user_id),
    use_case: AskQuestionUseCase = Depends(get_ask_question_use_case),
):
    try:
        answer = await use_case.execute(question=payload.question, session_id=str(user_id))
    except DomainError as exc:
        logger.exception("Domain error while answering question")
        raise HTTPException(status_code=400, detail=str(exc)) from exc
    except Exception as exc:  # noqa: BLE001
        logger.exception("Unexpected error while answering question")
        raise HTTPException(status_code=500, detail="Internal server error") from exc

    return AskResponse(
        answer=answer.content,
        sources=[SourceSchema(**s.__dict__) for s in answer.sources],
    )


@router.get("/history", response_model=ChatHistoryResponse)
async def get_chat_history(
    user_id: UUID = Depends(get_current_user_id),
    cache_service: CacheService = Depends(get_cache_service),
):
    history = await cache_service.get_history(str(user_id))
    return ChatHistoryResponse(
        user_id=user_id,
        messages=[ChatMessageSchema(**m) for m in history],
    )