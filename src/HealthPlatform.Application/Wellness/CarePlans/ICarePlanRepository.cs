using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness.CarePlans;

public sealed record CarePlanTaskDto(
    Guid Id,
    string Title,
    string? Description,
    DateOnly DueDate,
    DateTime? CompletedAtUtc,
    DateTime? ReminderSentAtUtc,
    bool IsCompleted);

public sealed record CarePlanMonitoringTargetDto(
    string MetricName,
    decimal TargetValue,
    string Unit);

public sealed record CarePlanProgressDto(
    int CompletedTaskCount,
    int TotalTaskCount,
    decimal PercentComplete);

public sealed record CarePlanDto(
    Guid Id,
    Guid PatientId,
    Guid DoctorId,
    string Condition,
    IReadOnlyList<CarePlanTaskDto> Tasks,
    IReadOnlyList<CarePlanMonitoringTargetDto> MonitoringTargets,
    int ReviewIntervalDays,
    DateOnly NextReviewAt,
    CarePlanStatus Status,
    CarePlanProgressDto Progress,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public interface ICarePlanRepository
{
    Task<CarePlan?> GetByIdAsync(Guid carePlanId, CancellationToken ct);

    Task<CarePlan?> GetByIdForPatientAsync(Guid carePlanId, Guid patientId, CancellationToken ct);

    Task<CarePlan?> GetByIdForDoctorAsync(Guid carePlanId, Guid doctorId, CancellationToken ct);

    Task<IReadOnlyList<CarePlan>> ListByPatientIdAsync(
        Guid patientId,
        CarePlanStatus? status,
        CancellationToken ct);

    Task<IReadOnlyList<CarePlan>> ListByDoctorIdAsync(
        Guid doctorId,
        CarePlanStatus? status,
        CancellationToken ct);

    Task<IReadOnlyList<CarePlan>> ListActiveForTaskRemindersAsync(int take, CancellationToken ct);

    Task<IReadOnlyList<CarePlan>> ListDueForReviewReminderAsync(DateOnly asOfDate, int take, CancellationToken ct);

    Task AddAsync(CarePlan carePlan, CancellationToken ct);

    Task UpdateAsync(CarePlan carePlan, CancellationToken ct);
}
