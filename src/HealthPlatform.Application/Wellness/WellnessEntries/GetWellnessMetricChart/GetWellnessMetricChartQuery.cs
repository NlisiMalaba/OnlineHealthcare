using HealthPlatform.Application.Behaviors;
using HealthPlatform.Application.Wellness.WellnessEntries;
using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness.WellnessEntries.GetWellnessMetricChart;

public sealed record GetWellnessMetricChartQuery(
    WellnessMetricType MetricType,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null) : IQuery<WellnessMetricChartDto>;
