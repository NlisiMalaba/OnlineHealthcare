using HealthPlatform.Domain.Prescriptions;

namespace HealthPlatform.Application.Prescriptions;

public interface IPrescriptionRepository
{
    Task AddAsync(Prescription prescription, CancellationToken ct);

    Task<Prescription?> GetByIdAsync(Guid prescriptionId, CancellationToken ct);
}
