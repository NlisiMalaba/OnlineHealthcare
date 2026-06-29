using HealthPlatform.Application.Insurance;
using StackExchange.Redis;

namespace HealthPlatform.Infrastructure.Insurance;

public sealed class RedisInsuranceClaimWebhookIdempotencyStore(IConnectionMultiplexer redis)
    : IInsuranceClaimWebhookIdempotencyStore
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromDays(7);

    public async Task<bool> TryBeginProcessingAsync(string insurerCode, string eventId, CancellationToken ct)
    {
        var key = $"insurance-webhook:{insurerCode}:{eventId}";
        return await redis.GetDatabase().StringSetAsync(key, "1", CacheTtl, When.NotExists);
    }
}
