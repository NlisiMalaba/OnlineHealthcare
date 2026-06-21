using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Identity;

public interface IPharmacyRepository
{
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct);

    Task<bool> ExistsByPhoneAsync(string phoneNumber, Guid? excludePharmacyId, CancellationToken ct);

    Task<Pharmacy?> GetByUserIdAsync(Guid userId, CancellationToken ct);

    Task AddAsync(Pharmacy pharmacy, CancellationToken ct);

    Task UpdateAsync(Pharmacy pharmacy, CancellationToken ct);
}
