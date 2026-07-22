using FluentValidation;

namespace HealthPlatform.Application.Wellness.HealthGoals.UpdateHealthGoal;

public sealed class UpdateHealthGoalCommandValidator : AbstractValidator<UpdateHealthGoalCommand>
{
    public UpdateHealthGoalCommandValidator()
    {
        RuleFor(command => command.GoalId).NotEmpty();

        RuleFor(command => command.TargetValue)
            .InclusiveBetween(WellnessPolicies.MinTargetValue, WellnessPolicies.MaxTargetValue);

        RuleFor(command => command.Unit)
            .NotEmpty()
            .MaximumLength(WellnessPolicies.MaxUnitLength);

        RuleFor(command => command.CustomLabel)
            .MaximumLength(WellnessPolicies.MaxCustomLabelLength)
            .When(command => !string.IsNullOrWhiteSpace(command.CustomLabel));
    }
}
