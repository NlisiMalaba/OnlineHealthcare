using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness.HealthGoals;

public sealed record HealthGoalDto(
    Guid Id,
    Guid PatientId,
    WellnessMetricType MetricType,
    decimal TargetValue,
    string Unit,
    string? CustomLabel,
    HealthGoalStatus Status,
    DateTime CreatedAtUtc);

public interface IHealthGoalRepository
{
    Task<IReadOnlyList<HealthGoal>> ListByPatientIdAsync(
        Guid patientId,
        HealthGoalStatus? status,
        CancellationToken ct);

    Task<IReadOnlyList<HealthGoal>> ListActiveByPatientAndMetricAsync(
        Guid patientId,
        WellnessMetricType metricType,
        CancellationToken ct);

    Task<HealthGoal?> GetByIdForPatientAsync(Guid goalId, Guid patientId, CancellationToken ct);

    Task AddAsync(HealthGoal goal, CancellationToken ct);

    Task UpdateAsync(HealthGoal goal, CancellationToken ct);

    Task DeleteAsync(HealthGoal goal, CancellationToken ct);
}
