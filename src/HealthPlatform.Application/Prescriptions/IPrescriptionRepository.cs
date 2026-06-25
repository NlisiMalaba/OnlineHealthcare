using HealthPlatform.Domain.Prescriptions;

namespace HealthPlatform.Application.Prescriptions;

public interface IPrescriptionRepository
{
    Task AddAsync(Prescription prescription, CancellationToken ct);

    Task<Prescription?> GetByIdAsync(Guid prescriptionId, CancellationToken ct);

    Task<Prescription?> GetByIdForPatientAsync(Guid prescriptionId, Guid patientId, CancellationToken ct);

    Task<Prescription?> GetByIdForDoctorAsync(Guid prescriptionId, Guid doctorId, CancellationToken ct);

    Task UpdateAsync(Prescription prescription, CancellationToken ct);
}
