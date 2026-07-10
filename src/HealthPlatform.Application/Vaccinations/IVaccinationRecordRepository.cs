using HealthPlatform.Domain.Vaccinations;

namespace HealthPlatform.Application.Vaccinations;

public interface IVaccinationRecordRepository
{
    Task AddAsync(VaccinationRecord record, CancellationToken ct);

    Task<IReadOnlyList<VaccinationRecord>> ListByChildProfileIdAsync(Guid childProfileId, CancellationToken ct);

    Task<IReadOnlyList<VaccinationRecord>> ListByPatientIdAsync(Guid patientId, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
