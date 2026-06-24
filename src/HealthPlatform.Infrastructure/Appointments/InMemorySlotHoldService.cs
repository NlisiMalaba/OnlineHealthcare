using System.Collections.Concurrent;
using HealthPlatform.Application.Appointments;

namespace HealthPlatform.Infrastructure.Appointments;

public sealed class InMemorySlotHoldService : ISlotHoldService
{
    private readonly ConcurrentDictionary<string, DateTime> _holds = new();

    public Task<bool> TryHoldAsync(Guid slotId, Guid patientId, TimeSpan ttl, CancellationToken ct)
    {
        var key = $"slot:{slotId}:hold";
        var now = DateTime.UtcNow;
        var expiresAt = now.Add(ttl);

        while (true)
        {
            if (_holds.TryGetValue(key, out var existingExpiry))
            {
                if (existingExpiry > now)
                {
                    return Task.FromResult(false);
                }

                _holds.TryRemove(key, out _);
                continue;
            }

            var added = _holds.TryAdd(key, expiresAt);
            return Task.FromResult(added);
        }
    }
}
