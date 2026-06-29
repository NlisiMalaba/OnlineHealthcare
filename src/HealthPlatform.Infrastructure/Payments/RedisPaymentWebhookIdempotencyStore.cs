using HealthPlatform.Application.Payments;
using StackExchange.Redis;

namespace HealthPlatform.Infrastructure.Payments;

public sealed class RedisPaymentWebhookIdempotencyStore(IConnectionMultiplexer redis) : IPaymentWebhookIdempotencyStore
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromDays(7);

    public async Task<bool> TryBeginProcessingAsync(string provider, string eventId, CancellationToken ct)
    {
        var key = BuildKey(provider, eventId);
        return await redis.GetDatabase().StringSetAsync(key, "1", CacheTtl, When.NotExists);
    }

    private static string BuildKey(string provider, string eventId) =>
        $"payment-webhook:{provider}:{eventId}";
}
