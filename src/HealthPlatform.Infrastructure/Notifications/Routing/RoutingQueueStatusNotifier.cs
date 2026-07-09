using HealthPlatform.Application.Notifications;
using HealthPlatform.Application.Queue;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingQueueStatusNotifier(INotificationDispatcher dispatcher) : IQueueStatusNotifier
{
    public Task NotifyMarkedAbsentAsync(
        Guid patientUserId,
        Guid queueEntryId,
        Guid appointmentId,
        CancellationToken ct)
    {
        var data = new Dictionary<string, string>
        {
            ["queue_entry_id"] = queueEntryId.ToString(),
            ["appointment_id"] = appointmentId.ToString(),
            ["status"] = "absent"
        };

        return NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            patientUserId,
            NotificationRecipientType.Patient,
            NotificationEventTypes.QueueMarkedAbsent,
            NotificationCriticality.Standard,
            "Queue status updated",
            "You were marked absent and removed from the clinic queue.",
            data,
            ct);
    }
}
