namespace HealthPlatform.API.Requests.Wellness;

public sealed class CarePlanTaskRequest
{
    public Guid? Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public DateOnly DueDate { get; init; }
}

public sealed class CarePlanMonitoringTargetRequest
{
    public string MetricName { get; init; } = string.Empty;

    public decimal TargetValue { get; init; }

    public string Unit { get; init; } = string.Empty;
}

public sealed class AssignCarePlanRequest
{
    public Guid PatientId { get; init; }

    public string Condition { get; init; } = string.Empty;

    public IReadOnlyList<CarePlanTaskRequest> Tasks { get; init; } = [];

    public IReadOnlyList<CarePlanMonitoringTargetRequest> MonitoringTargets { get; init; } = [];

    public int ReviewIntervalDays { get; init; }
}

public sealed class UpdateCarePlanRequest
{
    public string Condition { get; init; } = string.Empty;

    public IReadOnlyList<CarePlanTaskRequest> Tasks { get; init; } = [];

    public IReadOnlyList<CarePlanMonitoringTargetRequest> MonitoringTargets { get; init; } = [];

    public int ReviewIntervalDays { get; init; }

    public DateOnly? NextReviewAt { get; init; }
}
