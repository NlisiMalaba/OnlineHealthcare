using HealthPlatform.Application.Appointments;
using StackExchange.Redis;

namespace HealthPlatform.Infrastructure.Appointments;

public sealed class RedisSlotHoldService(IConnectionMultiplexer redis) : ISlotHoldService
{
    public async Task<bool> TryHoldAsync(Guid slotId, Guid patientId, TimeSpan ttl, CancellationToken ct)
    {
        var key = $"slot:{slotId}:hold";
        var db = redis.GetDatabase();
        return await db.StringSetAsync(
            key,
            patientId.ToString("N"),
            expiry: ttl,
            when: When.NotExists);
    }

    public async Task ReleaseHoldAsync(Guid slotId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var key = $"slot:{slotId}:hold";
        var db = redis.GetDatabase();
        await db.KeyDeleteAsync(key);
    }

    public async Task<bool> IsSlotHeldAsync(Guid slotId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var key = $"slot:{slotId}:hold";
        var db = redis.GetDatabase();
        return await db.KeyExistsAsync(key);
    }
}
