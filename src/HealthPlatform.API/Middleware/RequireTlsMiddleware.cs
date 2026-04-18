using Microsoft.Extensions.Options;

namespace HealthPlatform.API.Middleware;

public sealed class RequireTlsMiddlewareOptions
{
    /// <summary>
    /// When true, non-TLS requests are rejected (honours X-Forwarded-Proto when forwarded headers run first).
    /// </summary>
    public bool EnforceOutsideDevelopment { get; set; } = true;
}

/// <summary>
/// Rejects plain HTTP in hosted environments where TLS is terminated at the edge (use forwarded headers).
/// </summary>
public sealed class RequireTlsMiddleware(
    RequestDelegate next,
    IHostEnvironment environment,
    IOptions<RequireTlsMiddlewareOptions> options)
{
    public Task InvokeAsync(HttpContext context)
    {
        if (!options.Value.EnforceOutsideDevelopment || environment.IsDevelopment())
        {
            return next(context);
        }

        if (IsHealthProbe(context.Request.Path))
        {
            return next(context);
        }

        if (!IsHttpsRequest(context))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "text/plain; charset=utf-8";
            return context.Response.WriteAsync("TLS is required for this endpoint.");
        }

        return next(context);
    }

    private static bool IsHealthProbe(PathString path) =>
        path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase);

    private static bool IsHttpsRequest(HttpContext context) =>
        context.Request.IsHttps
        || string.Equals(context.Request.Headers["X-Forwarded-Proto"], "https", StringComparison.OrdinalIgnoreCase);
}
