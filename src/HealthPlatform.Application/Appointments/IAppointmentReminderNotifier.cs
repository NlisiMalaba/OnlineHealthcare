namespace HealthPlatform.Application.Appointments;

public interface IAppointmentReminderNotifier
{
    Task NotifyAppointmentReminderAsync(
        Guid patientUserId,
        Guid doctorUserId,
        Guid appointmentId,
        DateTime scheduledAtUtc,
        CancellationToken ct);
}
