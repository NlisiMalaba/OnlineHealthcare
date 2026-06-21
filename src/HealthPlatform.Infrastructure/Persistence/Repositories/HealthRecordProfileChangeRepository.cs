using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class HealthRecordProfileChangeRepository(ApplicationDbContext db)
    : IHealthRecordProfileChangeRepository
{
    public async Task AddRangeAsync(IReadOnlyList<HealthRecordProfileChange> changes, CancellationToken ct)
    {
        if (changes.Count == 0)
        {
            return;
        }

        await db.HealthRecordProfileChanges.AddRangeAsync(changes, ct);
    }
}
