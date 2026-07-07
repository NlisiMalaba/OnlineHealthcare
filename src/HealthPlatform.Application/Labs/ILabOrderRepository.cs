using HealthPlatform.Domain.Labs;

namespace HealthPlatform.Application.Labs;

public interface ILabOrderRepository
{
    Task AddAsync(LabOrder order, CancellationToken ct);

    Task<LabOrder?> GetByIdAsync(Guid labOrderId, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
