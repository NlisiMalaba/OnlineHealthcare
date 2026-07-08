using HealthPlatform.Domain.Referrals;

namespace HealthPlatform.API.Requests.Referrals;

public sealed class RespondToReferralRequest
{
    public ReferralResponseAction Action { get; init; }

    public string? Reason { get; init; }
}
