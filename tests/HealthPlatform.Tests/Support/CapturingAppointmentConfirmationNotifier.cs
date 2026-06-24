using HealthPlatform.Application.Appointments;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingAppointmentConfirmationNotifier : IAppointmentConfirmationNotifier
{
    public List<AppointmentConfirmationCall> Calls { get; } = [];

    public Task NotifyAppointmentConfirmedAsync(
        Guid patientUserId,
        Guid doctorUserId,
        Guid appointmentId,
        DateTime scheduledAtUtc,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Calls.Add(new AppointmentConfirmationCall(
            patientUserId,
            doctorUserId,
            appointmentId,
            scheduledAtUtc));
        return Task.CompletedTask;
    }

    public sealed record AppointmentConfirmationCall(
        Guid PatientUserId,
        Guid DoctorUserId,
        Guid AppointmentId,
        DateTime ScheduledAtUtc);
}
