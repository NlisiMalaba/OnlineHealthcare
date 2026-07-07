using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Notifications;
using HealthPlatform.Infrastructure.Notifications.Routing;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingAppointmentConfirmationNotifier(INotificationDispatcher dispatcher)
    : IAppointmentConfirmationNotifier
{
    public async Task NotifyAppointmentConfirmedAsync(
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
            NotificationEventTypes.AppointmentConfirmed,
            NotificationCriticality.Standard,
            "Appointment confirmed",
            "Your appointment has been confirmed.",
            data,
            ct);

        await NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            doctorUserId,
            NotificationRecipientType.Doctor,
            NotificationEventTypes.AppointmentConfirmed,
            NotificationCriticality.Standard,
            "Appointment confirmed",
            "A patient appointment has been confirmed.",
            data,
            ct);
    }
}
