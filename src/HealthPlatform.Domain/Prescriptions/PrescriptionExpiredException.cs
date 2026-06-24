namespace HealthPlatform.Domain.Prescriptions;

public sealed class PrescriptionExpiredException(Guid prescriptionId)
    : Exception($"Prescription '{prescriptionId}' has expired.")
{
    public Guid PrescriptionId { get; } = prescriptionId;
}
