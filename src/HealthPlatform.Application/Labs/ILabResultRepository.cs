using HealthPlatform.Domain.Labs;

namespace HealthPlatform.Application.Labs;

public interface ILabResultRepository
{
    Task AddAsync(LabResult result, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
