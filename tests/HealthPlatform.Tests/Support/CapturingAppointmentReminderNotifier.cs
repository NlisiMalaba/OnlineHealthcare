using HealthPlatform.Application.Appointments;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingAppointmentReminderNotifier : IAppointmentReminderNotifier
{
    public List<AppointmentReminderCall> Calls { get; } = [];

    public Task NotifyAppointmentReminderAsync(
        Guid patientUserId,
        Guid doctorUserId,
        Guid appointmentId,
        DateTime scheduledAtUtc,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Calls.Add(new AppointmentReminderCall(
            patientUserId,
            doctorUserId,
            appointmentId,
            scheduledAtUtc));
        return Task.CompletedTask;
    }

    public sealed record AppointmentReminderCall(
        Guid PatientUserId,
        Guid DoctorUserId,
        Guid AppointmentId,
        DateTime ScheduledAtUtc);
}
