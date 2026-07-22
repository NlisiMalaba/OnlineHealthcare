using HealthPlatform.Application.Behaviors;
using HealthPlatform.Application.Wellness.WellnessEntries;
using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness.WellnessEntries.RecordWellnessEntry;

public sealed record RecordWellnessEntryCommand(
    WellnessMetricType MetricType,
    decimal Value,
    Guid? GoalId,
    DateTime? RecordedAtUtc) : ICommand<WellnessEntryDto>;
