namespace HealthPlatform.Domain.Referrals;

public sealed class ReferralCompletionNotAllowedException(Guid referralId, ReferralStatus status)
    : Exception($"Referral '{referralId}' cannot be completed when status is '{status}'.")
{
    public Guid ReferralId { get; } = referralId;

    public ReferralStatus Status { get; } = status;
}
