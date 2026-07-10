using HealthPlatform.Domain.Maternal;

namespace HealthPlatform.Application.Maternal.GrowthEntries;

public interface IChildGrowthOutOfRangeNotifier
{
    Task NotifyGuardianAsync(
        Guid guardianUserId,
        Guid childProfileId,
        Guid growthEntryId,
        string childFullName,
        ChildGrowthMeasurementStatus heightStatus,
        ChildGrowthMeasurementStatus weightStatus,
        CancellationToken ct);
}
