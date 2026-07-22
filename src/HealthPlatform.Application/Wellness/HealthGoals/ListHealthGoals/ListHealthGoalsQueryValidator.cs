using FluentValidation;
using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness.HealthGoals.ListHealthGoals;

public sealed class ListHealthGoalsQueryValidator : AbstractValidator<ListHealthGoalsQuery>
{
    public ListHealthGoalsQueryValidator()
    {
        RuleFor(query => query.Status)
            .IsInEnum()
            .When(query => query.Status.HasValue);
    }
}
