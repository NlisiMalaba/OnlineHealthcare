using HealthPlatform.Application.Maternal.GrowthEntries;
using HealthPlatform.Domain.Maternal;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Maternal;

public sealed class LoggingChildGrowthOutOfRangeNotifier(
    ILogger<LoggingChildGrowthOutOfRangeNotifier> logger) : IChildGrowthOutOfRangeNotifier
{
    public Task NotifyGuardianAsync(
        Guid guardianUserId,
        Guid childProfileId,
        Guid growthEntryId,
        string childFullName,
        ChildGrowthMeasurementStatus heightStatus,
        ChildGrowthMeasurementStatus weightStatus,
        CancellationToken ct)
    {
        logger.LogInformation(
            "Child growth out-of-range alert for guardian user {GuardianUserId}, child profile {ChildProfileId}, entry {GrowthEntryId}, child {ChildFullName}, height status {HeightStatus}, weight status {WeightStatus}.",
            guardianUserId,
            childProfileId,
            growthEntryId,
            childFullName,
            heightStatus,
            weightStatus);

        return Task.CompletedTask;
    }
}
