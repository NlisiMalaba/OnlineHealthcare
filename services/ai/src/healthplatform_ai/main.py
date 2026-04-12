"""FastAPI entrypoint with structured logging and OpenTelemetry."""

from __future__ import annotations

import logging
import os
import sys

import structlog
import uvicorn
from fastapi import FastAPI
from opentelemetry import trace
from opentelemetry.exporter.otlp.proto.http.trace_exporter import OTLPSpanExporter
from opentelemetry.instrumentation.fastapi import FastAPIInstrumentor
from opentelemetry.sdk.resources import Resource
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor


def _configure_logging() -> None:
    logging.basicConfig(format="%(message)s", stream=sys.stdout, level=logging.INFO)
    structlog.configure(
        processors=[
            structlog.processors.TimeStamper(fmt="iso", key="ts"),
            structlog.processors.add_log_level,
            structlog.processors.StackInfoRenderer(),
            structlog.processors.format_exc_info,
            structlog.processors.JSONRenderer(),
        ],
        wrapper_class=structlog.make_filtering_bound_logger(logging.INFO),
        logger_factory=structlog.PrintLoggerFactory(file=sys.stdout),
    )


def _configure_otel(service_name: str) -> None:
    if not os.environ.get("OTEL_EXPORTER_OTLP_ENDPOINT"):
        return
    resource = Resource.create({"service.name": service_name})
    provider = TracerProvider(resource=resource)
    exporter = OTLPSpanExporter()
    provider.add_span_processor(BatchSpanProcessor(exporter))
    trace.set_tracer_provider(provider)


_configure_logging()
_configure_otel(os.environ.get("OTEL_SERVICE_NAME", "healthplatform-ai"))

app = FastAPI(title="HealthPlatform.AI", version="0.1.0")


@app.get("/health")
def health() -> dict[str, str]:
    return {"status": "healthy"}


FastAPIInstrumentor.instrument_app(app)


def run() -> None:
    """CLI entrypoint for `healthplatform-ai` script."""
    uvicorn.run(
        "healthplatform_ai.main:app",
        host="0.0.0.0",
        port=int(os.environ.get("PORT", "8081")),
        factory=False,
        reload=False,
    )


if __name__ == "__main__":
    run()
