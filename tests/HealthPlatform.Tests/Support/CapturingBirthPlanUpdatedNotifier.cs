using HealthPlatform.Application.Maternal.BirthPlans;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingBirthPlanUpdatedNotifier : IBirthPlanUpdatedNotifier
{
    public List<Call> Calls { get; } = [];

    public Task NotifyBirthPlanUpdatedAsync(
        Guid obstetricDoctorUserId,
        Guid birthPlanId,
        Guid antenatalRecordId,
        Guid patientId,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Calls.Add(new Call(obstetricDoctorUserId, birthPlanId, antenatalRecordId, patientId));
        return Task.CompletedTask;
    }

    public sealed record Call(
        Guid ObstetricDoctorUserId,
        Guid BirthPlanId,
        Guid AntenatalRecordId,
        Guid PatientId);
}
