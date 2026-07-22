using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.API.Requests.Wellness;

public sealed class CreateHealthGoalRequest
{
    public required WellnessMetricType MetricType { get; init; }

    public required decimal TargetValue { get; init; }

    public string? Unit { get; init; }

    public string? CustomLabel { get; init; }
}
