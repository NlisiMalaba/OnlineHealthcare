using HealthPlatform.Application.Prescriptions;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingPrescriptionIssuedNotifier : IPrescriptionIssuedNotifier
{
    public List<PrescriptionIssuedCall> Calls { get; } = [];

    public Task NotifyPrescriptionIssuedAsync(
        Guid patientUserId,
        Guid prescriptionId,
        DateTime issuedAtUtc,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Calls.Add(new PrescriptionIssuedCall(patientUserId, prescriptionId, issuedAtUtc));
        return Task.CompletedTask;
    }

    public sealed record PrescriptionIssuedCall(
        Guid PatientUserId,
        Guid PrescriptionId,
        DateTime IssuedAtUtc);
}
