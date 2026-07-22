using FluentValidation;
using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness.HealthGoals.CreateHealthGoal;

public sealed class CreateHealthGoalCommandValidator : AbstractValidator<CreateHealthGoalCommand>
{
    public CreateHealthGoalCommandValidator()
    {
        RuleFor(command => command.MetricType)
            .IsInEnum();

        RuleFor(command => command.TargetValue)
            .InclusiveBetween(WellnessPolicies.MinTargetValue, WellnessPolicies.MaxTargetValue);

        RuleFor(command => command.Unit)
            .MaximumLength(WellnessPolicies.MaxUnitLength)
            .When(command => !string.IsNullOrWhiteSpace(command.Unit));

        RuleFor(command => command.Unit)
            .NotEmpty()
            .MaximumLength(WellnessPolicies.MaxUnitLength)
            .When(command => command.MetricType == WellnessMetricType.Custom);

        RuleFor(command => command.CustomLabel)
            .NotEmpty()
            .MaximumLength(WellnessPolicies.MaxCustomLabelLength)
            .When(command => command.MetricType == WellnessMetricType.Custom);

        RuleFor(command => command.CustomLabel)
            .MaximumLength(WellnessPolicies.MaxCustomLabelLength)
            .When(command =>
                command.MetricType != WellnessMetricType.Custom
                && !string.IsNullOrWhiteSpace(command.CustomLabel));
    }
}
