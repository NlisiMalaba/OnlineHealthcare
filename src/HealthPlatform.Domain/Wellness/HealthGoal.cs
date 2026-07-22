using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.Wellness;

public sealed class HealthGoal : Entity
{
    private HealthGoal()
    {
        Unit = string.Empty;
    }

    public Guid PatientId { get; private set; }

    public WellnessMetricType MetricType { get; private set; }

    public decimal TargetValue { get; private set; }

    public string Unit { get; private set; }

    public string? CustomLabel { get; private set; }

    public HealthGoalStatus Status { get; private set; }

    public static HealthGoal Create(
        Guid patientId,
        WellnessMetricType metricType,
        decimal targetValue,
        string unit,
        string? customLabel,
        DateTime createdAtUtc)
    {
        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (targetValue <= 0)
        {
            throw new ArgumentException("Target value must be positive.", nameof(targetValue));
        }

        if (string.IsNullOrWhiteSpace(unit))
        {
            throw new ArgumentException("Unit is required.", nameof(unit));
        }

        if (metricType == WellnessMetricType.Custom && string.IsNullOrWhiteSpace(customLabel))
        {
            throw new ArgumentException("Custom label is required for custom metrics.", nameof(customLabel));
        }

        if (createdAtUtc == default || createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Creation timestamp must be UTC.", nameof(createdAtUtc));
        }

        return new HealthGoal
        {
            Id = Guid.CreateVersion7(),
            PatientId = patientId,
            MetricType = metricType,
            TargetValue = targetValue,
            Unit = unit.Trim(),
            CustomLabel = string.IsNullOrWhiteSpace(customLabel) ? null : customLabel.Trim(),
            Status = HealthGoalStatus.Active,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc
        };
    }

    public void Update(decimal targetValue, string unit, string? customLabel)
    {
        if (targetValue <= 0)
        {
            throw new ArgumentException("Target value must be positive.", nameof(targetValue));
        }

        if (string.IsNullOrWhiteSpace(unit))
        {
            throw new ArgumentException("Unit is required.", nameof(unit));
        }

        if (MetricType == WellnessMetricType.Custom && string.IsNullOrWhiteSpace(customLabel))
        {
            throw new ArgumentException("Custom label is required for custom metrics.", nameof(customLabel));
        }

        TargetValue = targetValue;
        Unit = unit.Trim();
        CustomLabel = string.IsNullOrWhiteSpace(customLabel) ? null : customLabel.Trim();
        Touch();
    }

    public void Complete()
    {
        Status = HealthGoalStatus.Completed;
        Touch();
    }

    public void Archive()
    {
        Status = HealthGoalStatus.Archived;
        Touch();
    }

    public void Reactivate()
    {
        Status = HealthGoalStatus.Active;
        Touch();
    }
}
