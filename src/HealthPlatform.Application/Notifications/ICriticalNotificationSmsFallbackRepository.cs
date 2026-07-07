using HealthPlatform.Domain.Notifications;

namespace HealthPlatform.Application.Notifications;

public interface ICriticalNotificationSmsFallbackRepository
{
    Task AddAsync(CriticalNotificationSmsFallback fallback, CancellationToken ct);

    Task<CriticalNotificationSmsFallback?> GetByIdAsync(Guid fallbackId, CancellationToken ct);

    Task<IReadOnlyList<CriticalNotificationSmsFallback>> ListDueAsync(
        DateTime asOfUtc,
        int batchSize,
        CancellationToken ct);

    Task UpdateAsync(CriticalNotificationSmsFallback fallback, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
