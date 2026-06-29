using System.Collections.Concurrent;
using HealthPlatform.Application.Payments;

namespace HealthPlatform.Infrastructure.Payments;

public sealed class InMemoryPaymentWebhookIdempotencyStore : IPaymentWebhookIdempotencyStore
{
    private readonly ConcurrentDictionary<string, byte> _processed = new(StringComparer.Ordinal);

    public Task<bool> TryBeginProcessingAsync(string provider, string eventId, CancellationToken ct)
    {
        var key = $"{provider}:{eventId}";
        return Task.FromResult(_processed.TryAdd(key, 0));
    }
}
