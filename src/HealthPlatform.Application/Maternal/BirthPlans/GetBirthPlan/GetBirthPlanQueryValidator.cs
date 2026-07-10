using FluentValidation;

namespace HealthPlatform.Application.Maternal.BirthPlans.GetBirthPlan;

public sealed class GetBirthPlanQueryValidator : AbstractValidator<GetBirthPlanQuery>
{
    public GetBirthPlanQueryValidator()
    {
        RuleFor(query => query.AntenatalRecordId)
            .NotEmpty();
    }
}
