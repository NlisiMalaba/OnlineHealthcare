using FluentValidation;

namespace HealthPlatform.Application.Wellness.HealthGoals.DeleteHealthGoal;

public sealed class DeleteHealthGoalCommandValidator : AbstractValidator<DeleteHealthGoalCommand>
{
    public DeleteHealthGoalCommandValidator()
    {
        RuleFor(command => command.GoalId).NotEmpty();
    }
}
