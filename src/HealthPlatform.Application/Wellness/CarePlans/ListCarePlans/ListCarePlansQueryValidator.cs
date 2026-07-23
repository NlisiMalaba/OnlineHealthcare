using FluentValidation;
using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness.CarePlans.ListCarePlans;

public sealed class ListCarePlansQueryValidator : AbstractValidator<ListCarePlansQuery>
{
    public ListCarePlansQueryValidator()
    {
        RuleFor(query => query.Status)
            .IsInEnum()
            .When(query => query.Status.HasValue);
    }
}
