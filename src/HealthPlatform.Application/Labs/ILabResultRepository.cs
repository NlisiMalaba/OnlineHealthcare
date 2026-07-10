using HealthPlatform.Domain.Labs;

namespace HealthPlatform.Application.Labs;

public interface ILabResultRepository
{
    Task<LabResult?> GetByIdAsync(Guid labResultId, CancellationToken ct);

    Task AddAsync(LabResult result, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
