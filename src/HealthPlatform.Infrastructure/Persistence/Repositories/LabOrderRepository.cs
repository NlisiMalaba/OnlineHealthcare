using HealthPlatform.Application.Labs;
using HealthPlatform.Domain.Labs;
using HealthPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class LabOrderRepository(ApplicationDbContext db) : ILabOrderRepository
{
    public Task AddAsync(LabOrder order, CancellationToken ct) =>
        db.LabOrders.AddAsync(order, ct).AsTask();

    public Task<LabOrder?> GetByIdAsync(Guid labOrderId, CancellationToken ct) =>
        db.LabOrders.FirstOrDefaultAsync(x => x.Id == labOrderId, ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
