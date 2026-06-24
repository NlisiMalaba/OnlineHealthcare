using HealthPlatform.Application.Prescriptions;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingPrescriptionCancelledNotifier : IPrescriptionCancelledNotifier
{
    public List<PrescriptionCancelledCall> Calls { get; } = [];

    public Task NotifyPrescriptionCancelledAsync(
        Guid patientUserId,
        Guid prescriptionId,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Calls.Add(new PrescriptionCancelledCall(patientUserId, prescriptionId));
        return Task.CompletedTask;
    }

    public sealed record PrescriptionCancelledCall(Guid PatientUserId, Guid PrescriptionId);
}
