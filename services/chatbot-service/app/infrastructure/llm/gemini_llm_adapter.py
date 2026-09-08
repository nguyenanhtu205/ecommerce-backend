from __future__ import annotations
import logging

from google import genai
from google.genai import types as genai_types

from app.application.interfaces.llm_service import LLMService
from app.domain.exceptions import LLMGenerationError

logger = logging.getLogger(__name__)

SYSTEM_PROMPT = (
    "Bạn là trợ lý hỗ trợ người dùng sử dụng hệ thống. "
    "Chỉ trả lời dựa trên phần NGỮ CẢNH được cung cấp. "
    "Nếu ngữ cảnh không đủ thông tin để trả lời, hãy nói rõ là bạn không tìm thấy "
    "thông tin liên quan trong tài liệu, đừng bịa đặt."
)


class GeminiLLMAdapter(LLMService):
    def __init__(self, api_key: str, model_name: str = "gemini-1.5-flash"):
        self._client = genai.Client(api_key=api_key)
        self._model_name = model_name

    async def generate_answer(
        self, question: str, context: str, chat_history: list[dict] | None = None
    ) -> str:
        history_text = ""
        if chat_history:
            history_text = "\n".join(
                f"{m['role']}: {m['content']}" for m in chat_history[-6:]
            )

        prompt = (
            f"LỊCH SỬ HỘI THOẠI (nếu có):\n{history_text}\n\n"
            f"NGỮ CẢNH TÀI LIỆU:\n{context}\n\n"
            f"CÂU HỎI: {question}\n\n"
            "Hãy trả lời ngắn gọn, chính xác, bằng tiếng Việt."
        )

        try:
            response = await self._client.aio.models.generate_content(
                model=self._model_name,
                contents=prompt,
                config=genai_types.GenerateContentConfig(system_instruction=SYSTEM_PROMPT),
            )
        except Exception as exc:  # noqa: BLE001
            logger.exception("Gemini generation failed")
            raise LLMGenerationError(str(exc)) from exc

        if response.text is None:
            finish_reason = None
            if response.candidates:
                finish_reason = response.candidates[0].finish_reason
            logger.warning(
                "Gemini trả về text=None cho question=%r, finish_reason=%s",
                question,
                finish_reason,
            )
            raise LLMGenerationError(
                f"Gemini không sinh được câu trả lời (finish_reason={finish_reason})"
            )

        return response.text