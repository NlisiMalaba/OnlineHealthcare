using HealthPlatform.Domain.Referrals;

namespace HealthPlatform.Application.Referrals;

public interface IReferralRepository
{
    Task AddAsync(Referral referral, CancellationToken ct);

    Task<Referral?> GetByIdAsync(Guid referralId, CancellationToken ct);

    Task UpdateAsync(Referral referral, CancellationToken ct);

    Task AddAccessGrantAsync(ReferralHealthRecordAccessGrant accessGrant, CancellationToken ct);
}
