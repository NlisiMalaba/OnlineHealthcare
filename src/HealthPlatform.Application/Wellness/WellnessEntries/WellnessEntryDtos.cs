using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness.WellnessEntries;

public sealed record HealthGoalProgressDto(
    Guid GoalId,
    WellnessMetricType MetricType,
    decimal TargetValue,
    decimal CurrentValue,
    string Unit,
    string? CustomLabel,
    decimal ProgressPercent,
    bool IsAchieved);

public sealed record WellnessEntryDto(
    Guid Id,
    Guid PatientId,
    Guid? GoalId,
    WellnessMetricType MetricType,
    decimal Value,
    DateTime RecordedAtUtc,
    IReadOnlyList<HealthGoalProgressDto> GoalProgress);

public sealed record WellnessMetricChartPointDto(DateTime RecordedAtUtc, decimal Value);

public sealed record WellnessMetricChartDto(
    WellnessMetricType MetricType,
    DateTime FromUtc,
    DateTime ToUtc,
    IReadOnlyList<WellnessMetricChartPointDto> Points);

public interface IWellnessEntryRepository
{
    Task AddAsync(WellnessEntry entry, CancellationToken ct);

    Task<IReadOnlyList<WellnessEntry>> ListByPatientIdAsync(
        Guid patientId,
        WellnessMetricType? metricType,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
