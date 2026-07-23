using FluentValidation;
using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness.CarePlans.ListDoctorCarePlans;

public sealed class ListDoctorCarePlansQueryValidator : AbstractValidator<ListDoctorCarePlansQuery>
{
    public ListDoctorCarePlansQueryValidator()
    {
        RuleFor(query => query.Status)
            .IsInEnum()
            .When(query => query.Status.HasValue);
    }
}
