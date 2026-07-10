using HealthPlatform.Application.Maternal.GrowthEntries;
using HealthPlatform.Domain.Maternal;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class GrowthEntryRepository(ApplicationDbContext db) : IGrowthEntryRepository
{
    public Task AddAsync(GrowthEntry entry, CancellationToken ct) =>
        db.GrowthEntries.AddAsync(entry, ct).AsTask();

    public async Task<IReadOnlyList<GrowthEntry>> ListByChildProfileIdAsync(
        Guid childProfileId,
        CancellationToken ct) =>
        await db.GrowthEntries
            .Where(entry => entry.ChildProfileId == childProfileId)
            .OrderBy(entry => entry.RecordedAtUtc)
            .ToListAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
