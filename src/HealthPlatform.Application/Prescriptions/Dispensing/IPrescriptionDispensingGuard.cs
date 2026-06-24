namespace HealthPlatform.Application.Prescriptions.Dispensing;

public interface IPrescriptionDispensingGuard
{
    Task<PrescriptionDto> DispenseForMedicationOrderAsync(
        Guid prescriptionId,
        Guid patientId,
        CancellationToken ct);
}
