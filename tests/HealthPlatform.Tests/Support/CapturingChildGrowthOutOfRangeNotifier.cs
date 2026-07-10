using HealthPlatform.Application.Maternal.GrowthEntries;
using HealthPlatform.Domain.Maternal;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingChildGrowthOutOfRangeNotifier : IChildGrowthOutOfRangeNotifier
{
    public List<Call> Calls { get; } = [];

    public Task NotifyGuardianAsync(
        Guid guardianUserId,
        Guid childProfileId,
        Guid growthEntryId,
        string childFullName,
        ChildGrowthMeasurementStatus heightStatus,
        ChildGrowthMeasurementStatus weightStatus,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Calls.Add(new Call(
            guardianUserId,
            childProfileId,
            growthEntryId,
            childFullName,
            heightStatus,
            weightStatus));
        return Task.CompletedTask;
    }

    public sealed record Call(
        Guid GuardianUserId,
        Guid ChildProfileId,
        Guid GrowthEntryId,
        string ChildFullName,
        ChildGrowthMeasurementStatus HeightStatus,
        ChildGrowthMeasurementStatus WeightStatus);
}
