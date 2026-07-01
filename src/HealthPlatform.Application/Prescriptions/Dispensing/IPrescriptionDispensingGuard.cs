using HealthPlatform.Domain.Prescriptions;

namespace HealthPlatform.Application.Prescriptions.Dispensing;

public interface IPrescriptionDispensingGuard
{
    Task<Prescription> PrepareDispenseForMedicationOrderAsync(
        Guid prescriptionId,
        Guid patientId,
        CancellationToken ct);

    Task PersistDispensedPrescriptionAsync(Prescription prescription, CancellationToken ct);

    Task<PrescriptionDto> DispenseForMedicationOrderAsync(
        Guid prescriptionId,
        Guid patientId,
        CancellationToken ct);
}
