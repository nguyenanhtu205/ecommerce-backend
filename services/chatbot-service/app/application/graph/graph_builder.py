from __future__ import annotations
from functools import partial

from langgraph.graph import StateGraph, START, END
from langgraph.graph.state import CompiledStateGraph

from app.application.graph.state import GraphState
from app.application.graph.tool_registry import ToolRegistry
from app.application.graph.nodes import (
    router_node,
    retrieve_node,
    generate_node,
    tool_execute_node,
    route_after_router,
)
from app.application.interfaces.embedding_service import EmbeddingService
from app.application.interfaces.vector_store_repository import VectorStoreRepository
from app.application.interfaces.llm_service import LLMService


def build_graph(
    embedding_service: EmbeddingService,
    vector_store: VectorStoreRepository,
    llm_service: LLMService,
    tool_registry: ToolRegistry,
    top_k: int = 4,
) -> CompiledStateGraph:
    graph = StateGraph(GraphState)

    graph.add_node("router", partial(router_node, tool_registry=tool_registry))
    graph.add_node(
        "retrieve",
        partial(
            retrieve_node,
            embedding_service=embedding_service,
            vector_store=vector_store,
            top_k=top_k,
        ),
    )
    graph.add_node("tool_execute", partial(tool_execute_node, tool_registry=tool_registry))
    graph.add_node("generate", partial(generate_node, llm_service=llm_service))

    graph.add_edge(START, "router")
    graph.add_conditional_edges(
        "router",
        route_after_router,
        {"retrieve": "retrieve", "tool_execute": "tool_execute"},
    )
    graph.add_edge("retrieve", "generate")
    graph.add_edge("tool_execute", "generate")
    graph.add_edge("generate", END)

    return graph.compile()