from collections.abc import Callable

from fastapi import FastAPI
from opentelemetry.instrumentation.fastapi import FastAPIInstrumentor

from health_platform_base.telemetry import configure_opentelemetry


def create_base_app(
    service_name: str,
    *,
    configure_routes: Callable[[FastAPI], None] | None = None,
) -> FastAPI:
    configure_opentelemetry(service_name)
    app = FastAPI(title=service_name)
    FastAPIInstrumentor.instrument_app(app)

    @app.get("/health")
    def health() -> dict[str, str]:
        return {"status": "healthy"}

    if configure_routes:
        configure_routes(app)

    return app
