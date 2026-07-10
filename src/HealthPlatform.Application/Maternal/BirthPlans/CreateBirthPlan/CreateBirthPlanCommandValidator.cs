using FluentValidation;
using HealthPlatform.Application.Maternal.BirthPlans;

namespace HealthPlatform.Application.Maternal.BirthPlans.CreateBirthPlan;

public sealed class CreateBirthPlanCommandValidator : AbstractValidator<CreateBirthPlanCommand>
{
    public CreateBirthPlanCommandValidator()
    {
        RuleFor(command => command.AntenatalRecordId)
            .NotEmpty();

        RuleFor(command => command.Content)
            .NotNull()
            .SetValidator(new BirthPlanContentDtoValidator());
    }
}

internal sealed class BirthPlanContentDtoValidator : AbstractValidator<BirthPlanContentDto>
{
    public BirthPlanContentDtoValidator()
    {
        RuleFor(content => content.LabourPreferences)
            .MaximumLength(BirthPlanPolicies.MaxPreferenceFieldLength)
            .When(content => !string.IsNullOrWhiteSpace(content.LabourPreferences));

        RuleFor(content => content.DeliveryMethod)
            .MaximumLength(BirthPlanPolicies.MaxPreferenceFieldLength)
            .When(content => !string.IsNullOrWhiteSpace(content.DeliveryMethod));

        RuleFor(content => content.PainManagement)
            .MaximumLength(BirthPlanPolicies.MaxPreferenceFieldLength)
            .When(content => !string.IsNullOrWhiteSpace(content.PainManagement));

        RuleFor(content => content.PostnatalCare)
            .MaximumLength(BirthPlanPolicies.MaxPreferenceFieldLength)
            .When(content => !string.IsNullOrWhiteSpace(content.PostnatalCare));

        RuleFor(content => content)
            .Must(content =>
                !string.IsNullOrWhiteSpace(content.LabourPreferences)
                || !string.IsNullOrWhiteSpace(content.DeliveryMethod)
                || !string.IsNullOrWhiteSpace(content.PainManagement)
                || !string.IsNullOrWhiteSpace(content.PostnatalCare))
            .WithMessage("At least one birth plan preference must be provided.");
    }
}
