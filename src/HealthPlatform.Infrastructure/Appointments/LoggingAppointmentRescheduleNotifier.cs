using HealthPlatform.Application.Appointments;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Appointments;

public sealed class LoggingAppointmentRescheduleNotifier(
    ILogger<LoggingAppointmentRescheduleNotifier> logger)
    : IAppointmentRescheduleNotifier
{
    public Task NotifyAppointmentRescheduledAsync(
        Guid patientUserId,
        Guid appointmentId,
        DateTime previousScheduledAtUtc,
        DateTime newScheduledAtUtc,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Appointment reschedule notification requested for appointment {AppointmentId}, patient user {PatientUserId}.",
            appointmentId,
            patientUserId);
        return Task.CompletedTask;
    }
}
