from __future__ import annotations
from contextlib import asynccontextmanager

from fastapi import FastAPI
from fastapi.responses import RedirectResponse

from app.infrastructure.config.settings import get_settings
from app.infrastructure.logging.logger import configure_logging
from app.infrastructure.persistence.db import create_pool
from app.presentation.api.dependencies import build_app_state_factory
from app.presentation.api.routers import ingest_router, chat_router, health_router

settings = get_settings()
configure_logging(settings.log_level)

build_state = build_app_state_factory()


@asynccontextmanager
async def lifespan(application: FastAPI):
    db_pool = await create_pool(settings.postgres_dsn)
    application.state.db_pool = db_pool

    state = await build_state(settings, db_pool)
    for key, value in state.items():
        setattr(application.state, key, value)

    yield

    await db_pool.close()


app = FastAPI(title=settings.app_name, lifespan=lifespan)

@app.get("/", include_in_schema=False)
async def root():
    return RedirectResponse(url="/docs")

app.include_router(health_router.router)

app.include_router(ingest_router.router)

app.include_router(chat_router.router)