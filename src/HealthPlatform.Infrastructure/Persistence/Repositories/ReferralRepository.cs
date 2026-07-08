using HealthPlatform.Application.Referrals;
using HealthPlatform.Domain.Referrals;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class ReferralRepository(ApplicationDbContext db) : IReferralRepository
{
    public async Task AddAsync(Referral referral, CancellationToken ct)
    {
        await db.Referrals.AddAsync(referral, ct);
        await db.SaveChangesAsync(ct);
    }
}
