using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Notifications;
using HealthPlatform.Infrastructure.Notifications.Routing;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingAppointmentReminderNotifier(INotificationDispatcher dispatcher)
    : IAppointmentReminderNotifier
{
    public async Task NotifyAppointmentReminderAsync(
        Guid patientUserId,
        Guid doctorUserId,
        Guid appointmentId,
        DateTime scheduledAtUtc,
        CancellationToken ct)
    {
        var data = new Dictionary<string, string>
        {
            ["appointment_id"] = appointmentId.ToString(),
            ["scheduled_at_utc"] = scheduledAtUtc.ToString("O")
        };

        await NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            patientUserId,
            NotificationRecipientType.Patient,
            NotificationEventTypes.AppointmentReminder,
            NotificationCriticality.Standard,
            "Appointment reminder",
            "You have an upcoming appointment.",
            data,
            ct);

        await NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            doctorUserId,
            NotificationRecipientType.Doctor,
            NotificationEventTypes.AppointmentReminder,
            NotificationCriticality.Standard,
            "Appointment reminder",
            "You have an upcoming patient appointment.",
            data,
            ct);
    }
}
