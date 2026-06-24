using System.Collections.Concurrent;
using HealthPlatform.Application.Appointments;

namespace HealthPlatform.Infrastructure.Appointments;

public sealed class InMemorySlotHoldService(TimeProvider timeProvider) : ISlotHoldService
{
    private readonly ConcurrentDictionary<string, DateTime> _holds = new();

    public Task<bool> TryHoldAsync(Guid slotId, Guid patientId, TimeSpan ttl, CancellationToken ct)
    {
        var key = $"slot:{slotId}:hold";
        var now = timeProvider.GetUtcNow().UtcDateTime;
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

    public Task ReleaseHoldAsync(Guid slotId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _holds.TryRemove($"slot:{slotId}:hold", out _);
        return Task.CompletedTask;
    }

    public Task<bool> IsSlotHeldAsync(Guid slotId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var key = $"slot:{slotId}:hold";
        if (!_holds.TryGetValue(key, out var existingExpiry))
        {
            return Task.FromResult(false);
        }

        if (existingExpiry <= timeProvider.GetUtcNow().UtcDateTime)
        {
            _holds.TryRemove(key, out _);
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}
