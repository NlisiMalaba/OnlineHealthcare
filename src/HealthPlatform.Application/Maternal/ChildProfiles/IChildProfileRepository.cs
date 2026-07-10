using HealthPlatform.Domain.Maternal;

namespace HealthPlatform.Application.Maternal.ChildProfiles;

public interface IChildProfileRepository
{
    Task AddAsync(ChildProfile childProfile, CancellationToken ct);

    Task<ChildProfile?> GetByIdAsync(Guid childProfileId, CancellationToken ct);

    Task<IReadOnlyList<ChildProfile>> ListByGuardianIdAsync(Guid guardianId, CancellationToken ct);
}
