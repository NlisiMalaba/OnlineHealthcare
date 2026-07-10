using FluentValidation;
using HealthPlatform.Application.Maternal.BirthPlans.CreateBirthPlan;

namespace HealthPlatform.Application.Maternal.BirthPlans.UpdateBirthPlan;

public sealed class UpdateBirthPlanCommandValidator : AbstractValidator<UpdateBirthPlanCommand>
{
    public UpdateBirthPlanCommandValidator()
    {
        RuleFor(command => command.AntenatalRecordId)
            .NotEmpty();

        RuleFor(command => command.Content)
            .NotNull()
            .SetValidator(new BirthPlanContentDtoValidator());
    }
}
