using HealthPlatform.Application.Notifications;
using HealthPlatform.Application.Wellness.CarePlans;
using HealthPlatform.Infrastructure.Notifications.Routing;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingCarePlanTaskDueReminderNotifier(INotificationDispatcher dispatcher)
    : ICarePlanTaskDueReminderNotifier
{
    public Task NotifyTaskDueAsync(
        Guid patientUserId,
        Guid carePlanId,
        Guid taskId,
        string condition,
        string taskTitle,
        DateOnly dueDate,
        CancellationToken ct) =>
        NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            patientUserId,
            NotificationRecipientType.Patient,
            NotificationEventTypes.CarePlanTaskDueReminder,
            NotificationCriticality.Standard,
            "Care plan task due",
            $"Your care plan task \"{taskTitle}\" is due on {dueDate:yyyy-MM-dd}.",
            new Dictionary<string, string>
            {
                ["care_plan_id"] = carePlanId.ToString(),
                ["task_id"] = taskId.ToString(),
                ["due_date"] = dueDate.ToString("O")
            },
            ct);
}

public sealed class RoutingCarePlanReviewReminderNotifier(INotificationDispatcher dispatcher)
    : ICarePlanReviewReminderNotifier
{
    public Task NotifyReviewDueAsync(
        Guid doctorUserId,
        Guid carePlanId,
        Guid patientId,
        string condition,
        DateOnly nextReviewAt,
        CancellationToken ct) =>
        NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            doctorUserId,
            NotificationRecipientType.Doctor,
            NotificationEventTypes.CarePlanReviewReminder,
            NotificationCriticality.Standard,
            "Care plan review due",
            $"A care plan review is due for patient progress (review date {nextReviewAt:yyyy-MM-dd}).",
            new Dictionary<string, string>
            {
                ["care_plan_id"] = carePlanId.ToString(),
                ["patient_id"] = patientId.ToString(),
                ["next_review_at"] = nextReviewAt.ToString("O")
            },
            ct);
}
