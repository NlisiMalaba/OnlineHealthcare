using HealthPlatform.Domain.NextOfKin;

namespace HealthPlatform.Application.NextOfKin;

public interface INextOfKinNotificationDeliveryRepository
{
    Task AddRangeAsync(IReadOnlyList<NextOfKinNotificationDelivery> deliveries, CancellationToken ct);

    Task<IReadOnlyList<NextOfKinNotificationDelivery>> ListDueForRetryAsync(
        DateTime asOfUtc,
        int batchSize,
        CancellationToken ct);

    Task UpdateAsync(NextOfKinNotificationDelivery delivery, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
