using HealthPlatform.Application.Maternal.ChildProfiles;
using HealthPlatform.Domain.Maternal;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class ChildProfileRepository(ApplicationDbContext db) : IChildProfileRepository
{
    public Task AddAsync(ChildProfile childProfile, CancellationToken ct) =>
        db.ChildProfiles.AddAsync(childProfile, ct).AsTask();

    public Task<ChildProfile?> GetByIdAsync(Guid childProfileId, CancellationToken ct) =>
        db.ChildProfiles.SingleOrDefaultAsync(profile => profile.Id == childProfileId, ct);

    public async Task<IReadOnlyList<ChildProfile>> ListByGuardianIdAsync(Guid guardianId, CancellationToken ct) =>
        await db.ChildProfiles
            .Where(profile => profile.GuardianId == guardianId)
            .OrderBy(profile => profile.FullName)
            .ToListAsync(ct);
}
