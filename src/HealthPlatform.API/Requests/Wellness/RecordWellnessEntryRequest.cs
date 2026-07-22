using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.API.Requests.Wellness;

public sealed class RecordWellnessEntryRequest
{
    public required WellnessMetricType MetricType { get; init; }

    public required decimal Value { get; init; }

    public Guid? GoalId { get; init; }

    public DateTime? RecordedAtUtc { get; init; }
}
