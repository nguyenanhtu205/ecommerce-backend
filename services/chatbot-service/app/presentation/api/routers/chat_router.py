from __future__ import annotations
import logging

from fastapi import APIRouter, Depends, HTTPException

from app.presentation.api.schemas.chat_schemas import AskRequest, AskResponse, SourceSchema
from app.presentation.api.dependencies import get_ask_question_use_case
from app.application.use_cases.ask_question import AskQuestionUseCase
from app.domain.exceptions import DomainError

logger = logging.getLogger(__name__)
router = APIRouter(prefix="/chat", tags=["chat"])


@router.post("/ask", response_model=AskResponse)
async def ask_question(
    payload: AskRequest,
    use_case: AskQuestionUseCase = Depends(get_ask_question_use_case),
):
    try:
        answer = await use_case.execute(question=payload.question, session_id=payload.session_id)
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