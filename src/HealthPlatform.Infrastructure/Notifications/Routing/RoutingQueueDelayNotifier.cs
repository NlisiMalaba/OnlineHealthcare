using HealthPlatform.Application.Notifications;
using HealthPlatform.Application.Queue;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingQueueDelayNotifier(INotificationDispatcher dispatcher) : IQueueDelayNotifier
{
    public Task NotifyQueueDelayRecalculatedAsync(
        Guid patientUserId,
        Guid queueEntryId,
        Guid appointmentId,
        int updatedEstimatedWaitMinutes,
        int delayMinutes,
        CancellationToken ct)
    {
        var data = new Dictionary<string, string>
        {
            ["queue_entry_id"] = queueEntryId.ToString(),
            ["appointment_id"] = appointmentId.ToString(),
            ["updated_estimated_wait_minutes"] = updatedEstimatedWaitMinutes.ToString(),
            ["delay_minutes"] = delayMinutes.ToString()
        };

        return NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            patientUserId,
            NotificationRecipientType.Patient,
            NotificationEventTypes.QueueDelayRecalculated,
            NotificationCriticality.Standard,
            "Queue delay update",
            $"Clinic delay updated your estimated wait to {updatedEstimatedWaitMinutes} minutes.",
            data,
            ct);
    }
}
