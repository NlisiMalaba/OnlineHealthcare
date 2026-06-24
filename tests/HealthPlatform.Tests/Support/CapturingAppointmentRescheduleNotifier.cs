using HealthPlatform.Application.Appointments;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingAppointmentRescheduleNotifier : IAppointmentRescheduleNotifier
{
    public List<AppointmentRescheduleCall> Calls { get; } = [];

    public Task NotifyAppointmentRescheduledAsync(
        Guid patientUserId,
        Guid appointmentId,
        DateTime previousScheduledAtUtc,
        DateTime newScheduledAtUtc,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Calls.Add(new AppointmentRescheduleCall(
            patientUserId,
            appointmentId,
            previousScheduledAtUtc,
            newScheduledAtUtc));
        return Task.CompletedTask;
    }

    public sealed record AppointmentRescheduleCall(
        Guid PatientUserId,
        Guid AppointmentId,
        DateTime PreviousScheduledAtUtc,
        DateTime NewScheduledAtUtc);
}
