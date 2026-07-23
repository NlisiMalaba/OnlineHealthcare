using FluentValidation;

namespace HealthPlatform.Application.Wellness.CarePlans.GetDoctorCarePlan;

public sealed class GetDoctorCarePlanQueryValidator : AbstractValidator<GetDoctorCarePlanQuery>
{
    public GetDoctorCarePlanQueryValidator()
    {
        RuleFor(query => query.CarePlanId).NotEmpty();
    }
}
