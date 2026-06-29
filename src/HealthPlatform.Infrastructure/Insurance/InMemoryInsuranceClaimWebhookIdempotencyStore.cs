using System.Collections.Concurrent;
using HealthPlatform.Application.Insurance;

namespace HealthPlatform.Infrastructure.Insurance;

public sealed class InMemoryInsuranceClaimWebhookIdempotencyStore : IInsuranceClaimWebhookIdempotencyStore
{
    private readonly ConcurrentDictionary<string, byte> _processed = new(StringComparer.Ordinal);

    public Task<bool> TryBeginProcessingAsync(string insurerCode, string eventId, CancellationToken ct)
    {
        var key = $"{insurerCode}:{eventId}";
        return Task.FromResult(_processed.TryAdd(key, 0));
    }
}
