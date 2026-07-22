using HealthPlatform.Application.Behaviors;
using HealthPlatform.Application.Wellness.WellnessEntries;
using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness.WellnessEntries.ListWellnessEntries;

public sealed record ListWellnessEntriesQuery(
    WellnessMetricType? MetricType = null,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null) : IQuery<IReadOnlyList<WellnessEntryDto>>;
