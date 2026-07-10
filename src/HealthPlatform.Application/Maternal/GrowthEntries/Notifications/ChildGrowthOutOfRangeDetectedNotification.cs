using HealthPlatform.Domain.Maternal;
using MediatR;

namespace HealthPlatform.Application.Maternal.GrowthEntries.Notifications;

public sealed record ChildGrowthOutOfRangeDetectedNotification(
    Guid GrowthEntryId,
    Guid ChildProfileId,
    Guid GuardianId,
    int AgeMonths,
    ChildGrowthMeasurementStatus HeightStatus,
    ChildGrowthMeasurementStatus WeightStatus,
    DateTime RecordedAtUtc,
    DateTime OccurredAtUtc) : INotification;
