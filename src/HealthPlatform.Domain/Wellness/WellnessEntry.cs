using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.Wellness;

public sealed class WellnessEntry : Entity
{
    private WellnessEntry()
    {
    }

    public Guid PatientId { get; private set; }

    public Guid? GoalId { get; private set; }

    public WellnessMetricType MetricType { get; private set; }

    public decimal Value { get; private set; }

    public DateTime RecordedAtUtc { get; private set; }

    public static WellnessEntry Create(
        Guid patientId,
        Guid? goalId,
        WellnessMetricType metricType,
        decimal value,
        DateTime recordedAtUtc,
        DateTime createdAtUtc)
    {
        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (goalId == Guid.Empty)
        {
            throw new ArgumentException("Goal id must be null or a non-empty id.", nameof(goalId));
        }

        if (value < 0)
        {
            throw new ArgumentException("Value cannot be negative.", nameof(value));
        }

        if (recordedAtUtc == default || recordedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Recorded timestamp must be UTC.", nameof(recordedAtUtc));
        }

        if (createdAtUtc == default || createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Creation timestamp must be UTC.", nameof(createdAtUtc));
        }

        return new WellnessEntry
        {
            Id = Guid.CreateVersion7(),
            PatientId = patientId,
            GoalId = goalId,
            MetricType = metricType,
            Value = value,
            RecordedAtUtc = recordedAtUtc,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc
        };
    }
}
