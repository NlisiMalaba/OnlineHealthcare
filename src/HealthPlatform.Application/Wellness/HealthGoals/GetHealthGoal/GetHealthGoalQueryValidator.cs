using FluentValidation;

namespace HealthPlatform.Application.Wellness.HealthGoals.GetHealthGoal;

public sealed class GetHealthGoalQueryValidator : AbstractValidator<GetHealthGoalQuery>
{
    public GetHealthGoalQueryValidator()
    {
        RuleFor(query => query.GoalId).NotEmpty();
    }
}
