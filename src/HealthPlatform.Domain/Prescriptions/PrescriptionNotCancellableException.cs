namespace HealthPlatform.Domain.Prescriptions;

public sealed class PrescriptionNotCancellableException(Guid prescriptionId, PrescriptionStatus status)
    : Exception($"Prescription '{prescriptionId}' in status '{status}' cannot be cancelled.")
{
    public Guid PrescriptionId { get; } = prescriptionId;

    public PrescriptionStatus Status { get; } = status;
}
