using HealthPlatform.Application.Notifications;
using HealthPlatform.Application.Wellness;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingMedicationScheduleCompletionNotifier(INotificationDispatcher dispatcher)
    : IMedicationScheduleCompletionNotifier
{
    public async Task NotifyScheduleCompletedAsync(
        MedicationScheduleCompletionNotice notice,
        CancellationToken ct)
    {
        var data = new Dictionary<string, string>
        {
            ["schedule_id"] = notice.ScheduleId.ToString(),
            ["prescription_id"] = notice.PrescriptionId.ToString(),
            ["completed_at_utc"] = notice.CompletedAtUtc.ToString("O")
        };

        await NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            notice.PatientUserId,
            NotificationRecipientType.Patient,
            NotificationEventTypes.MedicationScheduleCompleted,
            NotificationCriticality.Standard,
            "Medication schedule completed",
            "Your medication schedule has been completed.",
            data,
            ct);

        await NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            notice.DoctorUserId,
            NotificationRecipientType.Doctor,
            NotificationEventTypes.MedicationScheduleCompleted,
            NotificationCriticality.Standard,
            "Patient medication schedule completed",
            "A patient medication schedule has been completed.",
            data,
            ct);
    }
}
