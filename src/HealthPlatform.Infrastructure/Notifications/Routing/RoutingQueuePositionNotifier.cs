using HealthPlatform.Application.Notifications;
using HealthPlatform.Application.Queue;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingQueuePositionNotifier(INotificationDispatcher dispatcher) : IQueuePositionNotifier
{
    public Task NotifySecondPositionReachedAsync(
        Guid patientUserId,
        Guid queueEntryId,
        Guid appointmentId,
        int estimatedWaitMinutes,
        CancellationToken ct)
    {
        var data = new Dictionary<string, string>
        {
            ["queue_entry_id"] = queueEntryId.ToString(),
            ["appointment_id"] = appointmentId.ToString(),
            ["queue_position"] = "2",
            ["estimated_wait_minutes"] = estimatedWaitMinutes.ToString()
        };

        return NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            patientUserId,
            NotificationRecipientType.Patient,
            NotificationEventTypes.QueuePositionTwoReached,
            NotificationCriticality.Standard,
            "Almost your turn",
            "You are now second in the queue. Please proceed to the clinic.",
            data,
            ct);
    }
}
