using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace HealthPlatform.Shared.Telemetry;

/// <summary>
/// Registers OpenTelemetry tracing and metrics with OTLP export for ASP.NET Core services.
/// Honors standard OTEL_* environment variables (e.g. OTEL_EXPORTER_OTLP_ENDPOINT).
/// </summary>
public static class OpenTelemetryHostingExtensions
{
    public static WebApplicationBuilder AddHealthPlatformOpenTelemetry(
        this WebApplicationBuilder builder,
        string serviceName)
    {
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(serviceName))
            .WithTracing(t => t
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter())
            .WithMetrics(m => m
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter());

        return builder;
    }
}
