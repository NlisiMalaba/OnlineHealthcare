using FluentValidation;

namespace HealthPlatform.Application.Payments.Instalments.GetInstalmentPlan;

public sealed class GetInstalmentPlanQueryValidator : AbstractValidator<GetInstalmentPlanQuery>
{
    public GetInstalmentPlanQueryValidator()
    {
        RuleFor(x => x.PlanId).NotEmpty();
    }
}
