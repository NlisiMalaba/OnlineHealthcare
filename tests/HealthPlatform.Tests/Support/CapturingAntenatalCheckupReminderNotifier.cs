using HealthPlatform.Application.Maternal.AntenatalRecords;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingAntenatalCheckupReminderNotifier : IAntenatalCheckupReminderNotifier
{
    public List<Call> Calls { get; } = [];

    public Task NotifyAntenatalCheckupReminderAsync(
        Guid patientUserId,
        Guid obstetricDoctorUserId,
        Guid antenatalRecordId,
        DateOnly estimatedDueDate,
        bool highFrequency,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Calls.Add(new Call(
            patientUserId,
            obstetricDoctorUserId,
            antenatalRecordId,
            estimatedDueDate,
            highFrequency));
        return Task.CompletedTask;
    }

    public sealed record Call(
        Guid PatientUserId,
        Guid ObstetricDoctorUserId,
        Guid AntenatalRecordId,
        DateOnly EstimatedDueDate,
        bool HighFrequency);
}
