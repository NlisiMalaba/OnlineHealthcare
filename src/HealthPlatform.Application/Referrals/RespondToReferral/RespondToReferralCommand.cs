using HealthPlatform.Application.Behaviors;
using HealthPlatform.Domain.Referrals;

namespace HealthPlatform.Application.Referrals.RespondToReferral;

public sealed record RespondToReferralCommand(
    Guid ReferralId,
    ReferralResponseAction Action,
    string? Reason) : ICommand<ReferralDto>;
