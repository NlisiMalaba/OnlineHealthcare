using HealthPlatform.Application.Wellness;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingMedicationDoseReminderNotifier : IMedicationDoseReminderNotifier
{
    public List<MedicationDoseReminderCall> Calls { get; } = [];

    public Task NotifyDoseReminderAsync(
        Guid patientUserId,
        Guid scheduleId,
        DateTime scheduledAtUtc,
        CancellationToken ct)
    {
        Calls.Add(new MedicationDoseReminderCall(patientUserId, scheduleId, scheduledAtUtc));
        return Task.CompletedTask;
    }

    public sealed record MedicationDoseReminderCall(
        Guid PatientUserId,
        Guid ScheduleId,
        DateTime ScheduledAtUtc);
}
