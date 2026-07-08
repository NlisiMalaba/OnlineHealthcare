using HealthPlatform.Application.Notifications;
using HealthPlatform.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class CriticalNotificationSmsFallbackRepository(ApplicationDbContext dbContext)
    : ICriticalNotificationSmsFallbackRepository
{
    public async Task AddAsync(CriticalNotificationSmsFallback fallback, CancellationToken ct)
    {
        await dbContext.CriticalNotificationSmsFallbacks.AddAsync(fallback, ct);
    }

    public Task<CriticalNotificationSmsFallback?> GetByIdAsync(Guid fallbackId, CancellationToken ct) =>
        dbContext.CriticalNotificationSmsFallbacks
            .FirstOrDefaultAsync(fallback => fallback.Id == fallbackId, ct);

    public async Task<IReadOnlyList<CriticalNotificationSmsFallback>> ListDueAsync(
        DateTime asOfUtc,
        int batchSize,
        CancellationToken ct) =>
        await dbContext.CriticalNotificationSmsFallbacks
            .Where(fallback =>
                (fallback.Status == CriticalNotificationSmsFallbackStatus.AwaitingProcessing
                 || fallback.Status == CriticalNotificationSmsFallbackStatus.AwaitingRetry)
                && fallback.NextRetryAtUtc.HasValue
                && fallback.NextRetryAtUtc.Value <= asOfUtc)
            .OrderBy(fallback => fallback.NextRetryAtUtc)
            .Take(batchSize)
            .ToListAsync(ct);

    public Task UpdateAsync(CriticalNotificationSmsFallback fallback, CancellationToken ct)
    {
        dbContext.CriticalNotificationSmsFallbacks.Update(fallback);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => dbContext.SaveChangesAsync(ct);
}
