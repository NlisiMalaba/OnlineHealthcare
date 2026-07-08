using HealthPlatform.Application.Labs;
using HealthPlatform.Domain.Labs;
using HealthPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class LabResultRepository(ApplicationDbContext db) : ILabResultRepository
{
    public Task<LabResult?> GetByIdAsync(Guid labResultId, CancellationToken ct) =>
        db.LabResults.FirstOrDefaultAsync(x => x.Id == labResultId, ct);

    public Task AddAsync(LabResult result, CancellationToken ct) =>
        db.LabResults.AddAsync(result, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
