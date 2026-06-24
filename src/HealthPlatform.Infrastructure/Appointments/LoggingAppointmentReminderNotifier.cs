using HealthPlatform.Application.Appointments;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Appointments;

public sealed class LoggingAppointmentReminderNotifier(
    ILogger<LoggingAppointmentReminderNotifier> logger)
    : IAppointmentReminderNotifier
{
    public Task NotifyAppointmentReminderAsync(
        Guid patientUserId,
        Guid doctorUserId,
        Guid appointmentId,
        DateTime scheduledAtUtc,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Appointment reminder notification requested for appointment {AppointmentId}, patient user {PatientUserId}, doctor user {DoctorUserId}, scheduled at {ScheduledAtUtc}.",
            appointmentId,
            patientUserId,
            doctorUserId,
            scheduledAtUtc);
        return Task.CompletedTask;
    }
}
