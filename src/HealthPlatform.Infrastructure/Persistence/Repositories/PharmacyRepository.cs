using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class PharmacyRepository(ApplicationDbContext db) : IPharmacyRepository
{
    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct) =>
        db.Pharmacies.AnyAsync(p => p.ContactEmail == email.Trim().ToLowerInvariant(), ct);

    public Task<bool> ExistsByPhoneAsync(string phoneNumber, Guid? excludePharmacyId, CancellationToken ct) =>
        db.Pharmacies.AnyAsync(
            p => p.ContactPhone == phoneNumber.Trim()
                && (excludePharmacyId == null || p.Id != excludePharmacyId),
            ct);

    public Task<Pharmacy?> GetByUserIdAsync(Guid userId, CancellationToken ct) =>
        db.Pharmacies.SingleOrDefaultAsync(p => p.UserId == userId, ct);

    public async Task AddAsync(Pharmacy pharmacy, CancellationToken ct)
    {
        await db.Pharmacies.AddAsync(pharmacy, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task UpdateAsync(Pharmacy pharmacy, CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
