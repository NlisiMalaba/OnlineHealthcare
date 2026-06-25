namespace HealthPlatform.Domain.Prescriptions;

public sealed class PrescriptionNotEligibleException(Guid prescriptionId, PrescriptionStatus status)
    : Exception($"Prescription '{prescriptionId}' in status '{status}' cannot be used for a medication order.")
{
    public Guid PrescriptionId { get; } = prescriptionId;

    public PrescriptionStatus Status { get; } = status;
}
