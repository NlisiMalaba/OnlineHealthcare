namespace HealthPlatform.Application.Appointments;

public interface IAppointmentRescheduleNotifier
{
    Task NotifyAppointmentRescheduledAsync(
        Guid patientUserId,
        Guid appointmentId,
        DateTime previousScheduledAtUtc,
        DateTime newScheduledAtUtc,
        CancellationToken ct);
}
