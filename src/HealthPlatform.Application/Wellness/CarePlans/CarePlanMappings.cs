using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness.CarePlans;

public static class CarePlanMappings
{
    public static CarePlanDto ToDto(this CarePlan plan) =>
        new(
            plan.Id,
            plan.PatientId,
            plan.DoctorId,
            plan.Condition,
            plan.Tasks.Select(ToDto).ToList(),
            plan.MonitoringTargets.Select(ToDto).ToList(),
            plan.ReviewIntervalDays,
            plan.NextReviewAt,
            plan.Status,
            plan.Progress.ToDto(),
            plan.CreatedAtUtc,
            plan.UpdatedAtUtc);

    public static CarePlanTaskDto ToDto(this CarePlanTask task) =>
        new(
            task.Id,
            task.Title,
            task.Description,
            task.DueDate,
            task.CompletedAtUtc,
            task.ReminderSentAtUtc,
            task.IsCompleted);

    public static CarePlanMonitoringTargetDto ToDto(this CarePlanMonitoringTarget target) =>
        new(target.MetricName, target.TargetValue, target.Unit);

    public static CarePlanProgressDto ToDto(this CarePlanProgress progress) =>
        new(progress.CompletedTaskCount, progress.TotalTaskCount, progress.PercentComplete);

    public static CarePlanTaskDraft ToDraft(this CarePlanTaskInput input) =>
        new(input.Id ?? Guid.Empty, input.Title, input.Description, input.DueDate);

    public static CarePlanMonitoringTargetDraft ToDraft(this CarePlanMonitoringTargetInput input) =>
        new(input.MetricName, input.TargetValue, input.Unit);
}

public sealed record CarePlanTaskInput(
    Guid? Id,
    string Title,
    string? Description,
    DateOnly DueDate);

public sealed record CarePlanMonitoringTargetInput(
    string MetricName,
    decimal TargetValue,
    string Unit);
