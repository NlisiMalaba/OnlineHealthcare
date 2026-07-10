using HealthPlatform.Domain.Referrals;

namespace HealthPlatform.Application.Referrals;

public interface IReferralRepository
{
    Task AddAsync(Referral referral, CancellationToken ct);

    Task<Referral?> GetByIdAsync(Guid referralId, CancellationToken ct);

    Task UpdateAsync(Referral referral, CancellationToken ct);

    Task<IReadOnlyList<Referral>> ListPendingForTimeoutReminderAsync(
        DateTime asOfUtc,
        TimeSpan pendingThreshold,
        CancellationToken ct);

    Task AddAccessGrantAsync(ReferralHealthRecordAccessGrant accessGrant, CancellationToken ct);

    Task<ReferralHealthRecordAccessGrant?> GetAccessGrantByReferralIdAsync(Guid referralId, CancellationToken ct);

    Task UpdateAccessGrantAsync(ReferralHealthRecordAccessGrant accessGrant, CancellationToken ct);
}
