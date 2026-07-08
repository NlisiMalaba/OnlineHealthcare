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

    public async Task<IReadOnlyList<Referral>> ListPendingForTimeoutReminderAsync(
        DateTime asOfUtc,
        TimeSpan pendingThreshold,
        CancellationToken ct)
    {
        var latestCreatedAtUtc = asOfUtc - pendingThreshold;
        return await db.Referrals
            .Where(r =>
                r.Status == ReferralStatus.Pending
                && r.TimeoutReminderSentAtUtc == null
                && r.CreatedAtUtc <= latestCreatedAtUtc)
            .OrderBy(r => r.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public async Task AddAccessGrantAsync(ReferralHealthRecordAccessGrant accessGrant, CancellationToken ct)
    {
        await db.ReferralHealthRecordAccessGrants.AddAsync(accessGrant, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task<ReferralHealthRecordAccessGrant?> GetAccessGrantByReferralIdAsync(Guid referralId, CancellationToken ct) =>
        db.ReferralHealthRecordAccessGrants.SingleOrDefaultAsync(g => g.ReferralId == referralId, ct);

    public Task UpdateAccessGrantAsync(ReferralHealthRecordAccessGrant accessGrant, CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
