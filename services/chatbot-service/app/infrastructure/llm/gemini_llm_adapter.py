from __future__ import annotations
import logging

from google import genai
from google.genai import types as genai_types

from app.application.interfaces.llm_service import LLMService
from app.domain.exceptions import LLMGenerationError

logger = logging.getLogger(__name__)

SYSTEM_PROMPT = (
    "Bạn là trợ lý hỗ trợ người dùng sử dụng hệ thống, chỉ trả lời dựa trên phần "
    "NGỮ CẢNH được cung cấp trong mỗi lượt hỏi. Tuyệt đối không dùng kiến thức "
    "bên ngoài, không suy đoán, không bịa thêm thông tin không có trong NGỮ CẢNH.\n\n"
    "QUY TẮC BẮT BUỘC:\n"
    "1. Trước khi trả lời, tự kiểm tra: nội dung câu hỏi có thực sự được NGỮ CẢNH "
    "trả lời hay không? Nếu KHÔNG — dù vì bất kỳ lý do nào dưới đây — bạn PHẢI trả lời "
    "đúng NGUYÊN VĂN câu sau, không thêm bớt, không giải thích gì khác:\n"
    "\"Mình chưa tìm thấy thông tin phù hợp để trả lời câu hỏi này. Bạn thử hỏi lại "
    "theo cách khác hoặc hỏi về một nội dung khác nhé.\"\n\n"
    "2. Các trường hợp PHẢI dùng câu trên:\n"
    "   - Câu hỏi không liên quan gì tới nội dung NGỮ CẢNH (hỏi linh tinh, ngoài chủ đề, "
    "trò chuyện phiếm, hỏi về bản thân bạn/AI, hỏi kiến thức tổng quát không có trong tài liệu).\n"
    "   - Câu hỏi vô nghĩa, gõ lung tung, spam ký tự, hoặc quá mơ hồ không thể xác định ý định.\n"
    "   - Câu hỏi có vẻ liên quan tới chủ đề chung của tài liệu, nhưng chi tiết cụ thể được "
    "hỏi lại không xuất hiện trong NGỮ CẢNH (ví dụ hỏi đúng tên hạng mục nhưng thiếu dữ kiện, "
    "hoặc hỏi 1 con số/mốc thời gian không có trong đoạn trích).\n"
    "   - NGỮ CẢNH trống hoặc không có đoạn nào đủ liên quan.\n\n"
    "3. TRƯỚC KHI kết luận là không có thông tin, hãy khoan dung với lỗi chính tả, viết tắt, "
    "gõ không dấu tiếng Việt, hoặc cách diễn đạt khác của cùng 1 ý — cố gắng hiểu đúng ý định "
    "câu hỏi rồi đối chiếu lại với NGỮ CẢNH trước khi quyết định fallback. Chỉ fallback khi, "
    "sau khi đã hiểu đúng ý người dùng, NGỮ CẢNH vẫn thực sự không có câu trả lời.\n\n"
    "4. Khi NGỮ CẢNH có đủ thông tin: trả lời ngắn gọn, chính xác, đúng số liệu/chi tiết như "
    "trong NGỮ CẢNH, không tự làm tròn hay đổi đơn vị, bằng tiếng Việt tự nhiên, dễ hiểu."
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