using FluentValidation;
using HealthPlatform.Domain.Referrals;

namespace HealthPlatform.Application.Referrals.RespondToReferral;

public sealed class RespondToReferralCommandValidator : AbstractValidator<RespondToReferralCommand>
{
    public RespondToReferralCommandValidator()
    {
        RuleFor(x => x.ReferralId)
            .NotEmpty();

        RuleFor(x => x.Action)
            .IsInEnum();

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(1000)
            .When(x => x.Action is ReferralResponseAction.Decline or ReferralResponseAction.RequestAdditionalInformation);

        RuleFor(x => x.Reason)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Reason));
    }
}
