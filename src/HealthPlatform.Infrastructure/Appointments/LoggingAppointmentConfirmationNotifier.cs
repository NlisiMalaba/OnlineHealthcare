using HealthPlatform.Application.Appointments;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Appointments;

public sealed class LoggingAppointmentConfirmationNotifier(
    ILogger<LoggingAppointmentConfirmationNotifier> logger)
    : IAppointmentConfirmationNotifier
{
    public Task NotifyAppointmentConfirmedAsync(
        Guid patientUserId,
        Guid doctorUserId,
        Guid appointmentId,
        DateTime scheduledAtUtc,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Appointment confirmation notification requested for appointment {AppointmentId}, patient user {PatientUserId}, doctor user {DoctorUserId}.",
            appointmentId,
            patientUserId,
            doctorUserId);
        return Task.CompletedTask;
    }
}
