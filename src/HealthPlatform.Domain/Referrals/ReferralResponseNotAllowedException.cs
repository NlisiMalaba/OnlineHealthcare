namespace HealthPlatform.Domain.Referrals;

public sealed class ReferralResponseNotAllowedException(Guid referralId, ReferralStatus status)
    : Exception($"Referral '{referralId}' cannot be responded to when status is '{status}'.")
{
    public Guid ReferralId { get; } = referralId;

    public ReferralStatus Status { get; } = status;
}
