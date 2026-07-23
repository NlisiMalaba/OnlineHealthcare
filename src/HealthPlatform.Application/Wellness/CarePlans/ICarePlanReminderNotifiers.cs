namespace HealthPlatform.Application.Wellness.CarePlans;

public interface ICarePlanTaskDueReminderNotifier
{
    Task NotifyTaskDueAsync(
        Guid patientUserId,
        Guid carePlanId,
        Guid taskId,
        string condition,
        string taskTitle,
        DateOnly dueDate,
        CancellationToken ct);
}

public interface ICarePlanReviewReminderNotifier
{
    Task NotifyReviewDueAsync(
        Guid doctorUserId,
        Guid carePlanId,
        Guid patientId,
        string condition,
        DateOnly nextReviewAt,
        CancellationToken ct);
}
