using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthPlatform.API.Controllers;

[ApiController]
[AllowAnonymous]
public sealed class HealthController(HealthCheckService healthCheckService) : ControllerBase
{
    [HttpGet("/")]
    public IActionResult Root() => Redirect("/health");

    [HttpGet("/health")]
    public async Task<IActionResult> HealthAsync(CancellationToken ct)
    {
        var report = await healthCheckService.CheckHealthAsync(ct);
        return HealthJson(report);
    }

    [HttpGet("/health/live")]
    public async Task<IActionResult> LiveAsync(CancellationToken ct)
    {
        var report = await healthCheckService.CheckHealthAsync(
            registration => registration.Tags.Contains("live"),
            ct);
        return HealthJson(report);
    }

    [HttpGet("/health/ready")]
    public async Task<IActionResult> ReadyAsync(CancellationToken ct)
    {
        var report = await healthCheckService.CheckHealthAsync(
            registration => registration.Tags.Contains("ready"),
            ct);
        return HealthJson(report);
    }

    private JsonResult HealthJson(HealthReport report) =>
        new(HealthCheckPayload.From(report))
        {
            ContentType = "application/json; charset=utf-8"
        };

    private sealed record HealthCheckPayload(
        string Status,
        TimeSpan Duration,
        IReadOnlyList<HealthCheckEntryPayload> Checks)
    {
        public static HealthCheckPayload From(HealthReport report) =>
            new(
                report.Status.ToString(),
                report.TotalDuration,
                report.Entries
                    .Select(e => new HealthCheckEntryPayload(
                        e.Key,
                        e.Value.Status.ToString(),
                        e.Value.Description,
                        e.Value.Duration,
                        e.Value.Exception?.Message))
                    .ToList());
    }

    private sealed record HealthCheckEntryPayload(
        string Name,
        string Status,
        string? Description,
        TimeSpan Duration,
        string? Exception);
}
