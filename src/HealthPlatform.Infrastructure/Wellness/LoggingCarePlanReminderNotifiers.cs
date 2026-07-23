using HealthPlatform.Application.Wellness.CarePlans;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Wellness;

public sealed class LoggingCarePlanTaskDueReminderNotifier(
    ILogger<LoggingCarePlanTaskDueReminderNotifier> logger) : ICarePlanTaskDueReminderNotifier
{
    public Task NotifyTaskDueAsync(
        Guid patientUserId,
        Guid carePlanId,
        Guid taskId,
        string condition,
        string taskTitle,
        DateOnly dueDate,
        CancellationToken ct)
    {
        _ = condition;
        _ = taskTitle;
        logger.LogInformation(
            "Care plan task due reminder for user {PatientUserId}, plan {CarePlanId}, task {TaskId}, due {DueDate}.",
            patientUserId,
            carePlanId,
            taskId,
            dueDate);

        return Task.CompletedTask;
    }
}

public sealed class LoggingCarePlanReviewReminderNotifier(
    ILogger<LoggingCarePlanReviewReminderNotifier> logger) : ICarePlanReviewReminderNotifier
{
    public Task NotifyReviewDueAsync(
        Guid doctorUserId,
        Guid carePlanId,
        Guid patientId,
        string condition,
        DateOnly nextReviewAt,
        CancellationToken ct)
    {
        _ = condition;
        logger.LogInformation(
            "Care plan review reminder for doctor user {DoctorUserId}, plan {CarePlanId}, patient {PatientId}, review {NextReviewAt}.",
            doctorUserId,
            carePlanId,
            patientId,
            nextReviewAt);

        return Task.CompletedTask;
    }
}
