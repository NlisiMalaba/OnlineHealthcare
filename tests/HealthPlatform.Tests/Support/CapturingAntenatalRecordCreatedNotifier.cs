using HealthPlatform.Application.Maternal.AntenatalRecords;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingAntenatalRecordCreatedNotifier : IAntenatalRecordCreatedNotifier
{
    public List<Call> Calls { get; } = [];

    public Task NotifyAntenatalRecordCreatedAsync(
        Guid patientUserId,
        Guid obstetricDoctorUserId,
        Guid antenatalRecordId,
        DateOnly estimatedDueDate,
        int recommendedCheckupCount,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Calls.Add(new Call(
            patientUserId,
            obstetricDoctorUserId,
            antenatalRecordId,
            estimatedDueDate,
            recommendedCheckupCount));
        return Task.CompletedTask;
    }

    public sealed record Call(
        Guid PatientUserId,
        Guid ObstetricDoctorUserId,
        Guid AntenatalRecordId,
        DateOnly EstimatedDueDate,
        int RecommendedCheckupCount);
}
