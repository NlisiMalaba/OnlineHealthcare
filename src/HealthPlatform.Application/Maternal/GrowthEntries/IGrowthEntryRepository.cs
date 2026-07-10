using HealthPlatform.Domain.Maternal;

namespace HealthPlatform.Application.Maternal.GrowthEntries;

public interface IGrowthEntryRepository
{
    Task AddAsync(GrowthEntry entry, CancellationToken ct);

    Task<IReadOnlyList<GrowthEntry>> ListByChildProfileIdAsync(Guid childProfileId, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
