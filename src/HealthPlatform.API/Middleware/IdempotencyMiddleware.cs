using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
namespace HealthPlatform.API.Middleware;

/// <summary>
/// Requires <c>Idempotency-Key</c> on POST payment/order routes and replays cached responses for duplicate keys (24h TTL in Redis).
/// </summary>
public sealed class IdempotencyMiddleware(RequestDelegate next, ILogger<IdempotencyMiddleware> logger)
{
    private const string IdempotencyKeyHeaderName = "Idempotency-Key";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly DistributedCacheEntryOptions CacheTtl = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
    };

    public async Task InvokeAsync(HttpContext context, IDistributedCache cache)
    {
        if (!ShouldEnforce(context.Request))
        {
            await next(context);
            return;
        }

        if (!TryGetIdempotencyKey(context.Request, out var rawKey))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/problem+json; charset=utf-8";
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Missing idempotency key",
                detail = "Header Idempotency-Key is required for this operation."
            });
            return;
        }

        var cacheKey = BuildCacheKey(context, rawKey);
        if (await cache.GetStringAsync(cacheKey, context.RequestAborted) is { } cachedJson)
        {
            var entry = JsonSerializer.Deserialize<IdempotencyCacheEntry>(cachedJson, JsonOptions);
            if (entry is not null)
            {
                context.Response.StatusCode = entry.StatusCode;
                if (!string.IsNullOrEmpty(entry.ContentType))
                {
                    context.Response.ContentType = entry.ContentType;
                }

                if (entry.BodyBase64 is { Length: > 0 } b64)
                {
                    var body = Convert.FromBase64String(b64);
                    await context.Response.Body.WriteAsync(body, context.RequestAborted);
                }

                return;
            }
        }

        var originalBody = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await next(context);
        }
        finally
        {
            context.Response.Body = originalBody;
        }

        buffer.Seek(0, SeekOrigin.Begin);
        var captured = buffer.ToArray();

        if (context.Response.StatusCode < 500 && captured.Length < 256 * 1024)
        {
            var entry = new IdempotencyCacheEntry(
                context.Response.StatusCode,
                context.Response.ContentType ?? "application/octet-stream",
                Convert.ToBase64String(captured));

            try
            {
                await cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(entry, JsonOptions),
                    CacheTtl,
                    context.RequestAborted);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to persist idempotency entry for key hash {Hash}.", cacheKey);
            }
        }

        context.Response.ContentLength = captured.Length;
        await originalBody.WriteAsync(captured, context.RequestAborted);
    }

    private static bool ShouldEnforce(HttpRequest request)
    {
        if (!HttpMethods.IsPost(request.Method))
        {
            return false;
        }

        var path = request.Path.Value ?? string.Empty;
        return path.Contains("payments", StringComparison.OrdinalIgnoreCase)
               || path.Contains("/orders", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryGetIdempotencyKey(HttpRequest request, out string key)
    {
        key = request.Headers[IdempotencyKeyHeaderName].ToString().Trim();
        return key.Length > 0 && key.Length <= 128;
    }

    private static string BuildCacheKey(HttpContext context, string rawKey)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes($"{context.Request.Method}:{path}:{rawKey}")));
        return $"idempotency:{hash}";
    }

    private sealed record IdempotencyCacheEntry(int StatusCode, string ContentType, string BodyBase64);
}
