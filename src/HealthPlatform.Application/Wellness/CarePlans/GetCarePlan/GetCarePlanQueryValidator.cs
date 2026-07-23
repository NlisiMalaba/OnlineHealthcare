using FluentValidation;

namespace HealthPlatform.Application.Wellness.CarePlans.GetCarePlan;

public sealed class GetCarePlanQueryValidator : AbstractValidator<GetCarePlanQuery>
{
    public GetCarePlanQueryValidator()
    {
        RuleFor(query => query.CarePlanId).NotEmpty();
    }
}
