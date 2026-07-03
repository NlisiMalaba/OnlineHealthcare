using HealthPlatform.Domain.HealthRecords;

namespace HealthPlatform.Application.Identity;

public interface IHealthRecordRepository
{
    Task<bool> ExistsForPatientAsync(Guid patientId, CancellationToken ct);

    Task<HealthRecord?> GetByIdAsync(Guid healthRecordId, CancellationToken ct);

    Task<HealthRecord?> GetByPatientIdAsync(Guid patientId, CancellationToken ct);

    Task AddAsync(HealthRecord healthRecord, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
