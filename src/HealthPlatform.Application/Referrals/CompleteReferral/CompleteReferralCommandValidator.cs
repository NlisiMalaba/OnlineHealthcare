using FluentValidation;

namespace HealthPlatform.Application.Referrals.CompleteReferral;

public sealed class CompleteReferralCommandValidator : AbstractValidator<CompleteReferralCommand>
{
    public CompleteReferralCommandValidator()
    {
        RuleFor(x => x.ReferralId)
            .NotEmpty();

        RuleFor(x => x.ConsultationSummary)
            .NotEmpty()
            .MaximumLength(4000);
    }
}
