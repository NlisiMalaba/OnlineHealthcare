import uvicorn

from health_platform_base import create_base_app

app = create_base_app("ai-analytics-insights")

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8103)
