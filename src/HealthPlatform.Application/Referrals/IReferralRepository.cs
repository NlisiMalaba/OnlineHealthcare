using HealthPlatform.Domain.Referrals;

namespace HealthPlatform.Application.Referrals;

public interface IReferralRepository
{
    Task AddAsync(Referral referral, CancellationToken ct);
}
