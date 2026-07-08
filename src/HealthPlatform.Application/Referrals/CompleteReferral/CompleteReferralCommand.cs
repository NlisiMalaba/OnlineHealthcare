using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Referrals.CompleteReferral;

public sealed record CompleteReferralCommand(
    Guid ReferralId,
    string ConsultationSummary) : ICommand<ReferralDto>;
