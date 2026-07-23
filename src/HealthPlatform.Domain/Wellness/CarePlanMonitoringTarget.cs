namespace HealthPlatform.Domain.Wellness;

public sealed record CarePlanMonitoringTarget(
    string MetricName,
    decimal TargetValue,
    string Unit);
