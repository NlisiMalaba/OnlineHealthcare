using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Domain.NextOfKin;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class NextOfKinNotificationDeliveryRepository(ApplicationDbContext db)
    : INextOfKinNotificationDeliveryRepository
{
    public async Task AddRangeAsync(IReadOnlyList<NextOfKinNotificationDelivery> deliveries, CancellationToken ct) =>
        await db.NextOfKinNotificationDeliveries.AddRangeAsync(deliveries, ct);

    public async Task<IReadOnlyList<NextOfKinNotificationDelivery>> ListDueForRetryAsync(
        DateTime asOfUtc,
        int batchSize,
        CancellationToken ct) =>
        await db.NextOfKinNotificationDeliveries
            .Where(delivery =>
                delivery.Status == NextOfKinNotificationDeliveryStatus.AwaitingRetry
                && delivery.NextRetryAtUtc.HasValue
                && delivery.NextRetryAtUtc.Value <= asOfUtc
                && delivery.RetryCount < NextOfKinPolicies.MaxNotificationRetries)
            .OrderBy(delivery => delivery.NextRetryAtUtc)
            .Take(batchSize)
            .ToListAsync(ct);

    public Task UpdateAsync(NextOfKinNotificationDelivery delivery, CancellationToken ct)
    {
        db.NextOfKinNotificationDeliveries.Update(delivery);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
