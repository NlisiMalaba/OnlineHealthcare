using HealthPlatform.Application.Referrals;
using HealthPlatform.Domain.Referrals;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class ReferralRepository(ApplicationDbContext db) : IReferralRepository
{
    public async Task AddAsync(Referral referral, CancellationToken ct)
    {
        await db.Referrals.AddAsync(referral, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task<Referral?> GetByIdAsync(Guid referralId, CancellationToken ct) =>
        db.Referrals.SingleOrDefaultAsync(r => r.Id == referralId, ct);

    public Task UpdateAsync(Referral referral, CancellationToken ct) =>
        db.SaveChangesAsync(ct);

    public async Task AddAccessGrantAsync(ReferralHealthRecordAccessGrant accessGrant, CancellationToken ct)
    {
        await db.ReferralHealthRecordAccessGrants.AddAsync(accessGrant, ct);
        await db.SaveChangesAsync(ct);
    }
}
