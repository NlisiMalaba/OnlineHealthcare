using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Maternal.Events;

public sealed record ChildGrowthOutOfRangeDetectedDomainEvent(
    Guid GrowthEntryId,
    Guid ChildProfileId,
    Guid GuardianId,
    int AgeMonths,
    ChildGrowthMeasurementStatus HeightStatus,
    ChildGrowthMeasurementStatus WeightStatus,
    DateTime RecordedAtUtc) : DomainEvent;
