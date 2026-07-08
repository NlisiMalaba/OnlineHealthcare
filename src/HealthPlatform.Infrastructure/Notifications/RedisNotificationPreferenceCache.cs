using System.Text.Json;
using HealthPlatform.Application.Notifications;
using Microsoft.Extensions.Caching.Distributed;

namespace HealthPlatform.Infrastructure.Notifications;

public sealed class RedisNotificationPreferenceCache(IDistributedCache cache) : INotificationPreferenceCache
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<StoredNotificationChannelPreference>?> GetAsync(
        Guid userId,
        CancellationToken ct)
    {
        var payload = await cache.GetStringAsync(CreateCacheKey(userId), ct);
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        return JsonSerializer.Deserialize<List<StoredNotificationChannelPreference>>(payload, SerializerOptions);
    }

    public Task SetAsync(
        Guid userId,
        IReadOnlyList<StoredNotificationChannelPreference> preferences,
        CancellationToken ct)
    {
        var payload = JsonSerializer.Serialize(preferences, SerializerOptions);
        return cache.SetStringAsync(
            CreateCacheKey(userId),
            payload,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = NotificationPreferencePolicies.CacheTtl
            },
            ct);
    }

    public Task InvalidateAsync(Guid userId, CancellationToken ct) =>
        cache.RemoveAsync(CreateCacheKey(userId), ct);

    private static string CreateCacheKey(Guid userId) => $"notification:preferences:{userId}";
}
