from __future__ import annotations
import logging

from app.application.graph.state import GraphState
from app.application.graph.tool_registry import ToolRegistry
from app.application.interfaces.embedding_service import EmbeddingService
from app.application.interfaces.vector_store_repository import VectorStoreRepository
from app.application.interfaces.llm_service import LLMService

logger = logging.getLogger(__name__)

RAG_TOOL_NAME = "rag_tool"


async def router_node(state: GraphState, tool_registry: ToolRegistry) -> GraphState:
    tool = await tool_registry.resolve(state["question"])
    if tool is None:
        tool = tool_registry.get_default()
    logger.info("Router selected tool=%s for question=%r", tool.name, state["question"])
    state["tool_context"] = ""
    state["selected_tool_name"] = tool.name
    return state


async def retrieve_node(
    state: GraphState,
    embedding_service: EmbeddingService,
    vector_store: VectorStoreRepository,
    top_k: int = 4,
) -> GraphState:
    query_embedding = await embedding_service.embed_query(state["question"])
    chunks = await vector_store.similarity_search(query_embedding, top_k=top_k)

    state["retrieved_chunks"] = [
        {
            "content": c.content,
            "source": c.source,
            "chunk_index": c.chunk_index,
            "score": c.score,
        }
        for c in chunks
    ]
    return state


async def generate_node(state: GraphState, llm_service: LLMService) -> GraphState:
    if state["retrieved_chunks"]:
        context = "\n\n".join(
            f"[Source: {c['source']} - chunk #{c['chunk_index']}]\n{c['content']}"
            for c in state["retrieved_chunks"]
        )
    else:
        context = state.get("tool_context", "")

    answer_text = await llm_service.generate_answer(
        question=state["question"],
        context=context,
        chat_history=state.get("chat_history"),
    )

    state["answer"] = answer_text
    state["sources"] = [
        {
            "source": c["source"],
            "chunk_index": c["chunk_index"],
            "snippet": c["content"][:200],
            "score": c["score"],
        }
        for c in state["retrieved_chunks"]
    ]
    return state


async def tool_execute_node(state: GraphState, tool_registry: ToolRegistry) -> GraphState:
    tool_name = state.get("selected_tool_name", RAG_TOOL_NAME)
    tool = tool_registry.get_by_name(tool_name)
    if tool is None:
        return state
    result = await tool.execute(state["question"], context={})
    state["tool_context"] = result
    return state


def route_after_router(state: GraphState) -> str:
    selected = state.get("selected_tool_name", RAG_TOOL_NAME)
    return "retrieve" if selected == RAG_TOOL_NAME else "tool_execute"