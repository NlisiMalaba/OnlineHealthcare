using HealthPlatform.Application.Maternal.AntenatalRecords;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingFetalMonitoringReminderNotifier : IFetalMonitoringReminderNotifier
{
    public List<Call> Calls { get; } = [];

    public Task NotifyFetalMonitoringReminderAsync(
        Guid patientUserId,
        Guid antenatalRecordId,
        int intervalDays,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Calls.Add(new Call(patientUserId, antenatalRecordId, intervalDays));
        return Task.CompletedTask;
    }

    public sealed record Call(Guid PatientUserId, Guid AntenatalRecordId, int IntervalDays);
}
