namespace HealthPlatform.Application.Appointments;

public interface IAppointmentConfirmationNotifier
{
    Task NotifyAppointmentConfirmedAsync(
        Guid patientUserId,
        Guid doctorUserId,
        Guid appointmentId,
        DateTime scheduledAtUtc,
        CancellationToken ct);
}
