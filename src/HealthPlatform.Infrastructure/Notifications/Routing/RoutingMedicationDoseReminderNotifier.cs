using HealthPlatform.Application.Notifications;
using HealthPlatform.Application.Wellness;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingMedicationDoseReminderNotifier(INotificationDispatcher dispatcher)
    : IMedicationDoseReminderNotifier
{
    public Task NotifyDoseReminderAsync(
        Guid patientUserId,
        Guid scheduleId,
        DateTime scheduledAtUtc,
        CancellationToken ct) =>
        NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            patientUserId,
            NotificationRecipientType.Patient,
            NotificationEventTypes.MedicationDoseReminder,
            NotificationCriticality.Critical,
            "Medication reminder",
            "It is time to take your medication.",
            new Dictionary<string, string>
            {
                ["schedule_id"] = scheduleId.ToString(),
                ["scheduled_at_utc"] = scheduledAtUtc.ToString("O")
            },
            ct);
}
