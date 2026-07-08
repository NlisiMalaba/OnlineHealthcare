using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Notifications;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingAppointmentRescheduleNotifier(INotificationDispatcher dispatcher)
    : IAppointmentRescheduleNotifier
{
    public Task NotifyAppointmentRescheduledAsync(
        Guid patientUserId,
        Guid appointmentId,
        DateTime previousScheduledAtUtc,
        DateTime newScheduledAtUtc,
        CancellationToken ct) =>
        NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            patientUserId,
            NotificationRecipientType.Patient,
            NotificationEventTypes.AppointmentRescheduled,
            NotificationCriticality.Standard,
            "Appointment rescheduled",
            "Your appointment has been rescheduled.",
            new Dictionary<string, string>
            {
                ["appointment_id"] = appointmentId.ToString(),
                ["previous_scheduled_at_utc"] = previousScheduledAtUtc.ToString("O"),
                ["new_scheduled_at_utc"] = newScheduledAtUtc.ToString("O")
            },
            ct);
}
