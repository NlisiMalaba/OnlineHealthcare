namespace HealthPlatform.Domain.Prescriptions;

public sealed class PrescriptionDispensedException(Guid prescriptionId)
    : Exception($"Prescription '{prescriptionId}' has already been dispensed.")
{
    public Guid PrescriptionId { get; } = prescriptionId;
}
