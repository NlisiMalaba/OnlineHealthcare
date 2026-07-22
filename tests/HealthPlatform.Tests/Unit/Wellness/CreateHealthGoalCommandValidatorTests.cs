using FluentValidation.TestHelper;
using HealthPlatform.Application.Wellness.HealthGoals.CreateHealthGoal;
using HealthPlatform.Domain.Wellness;
using Xunit;

namespace HealthPlatform.Tests.Unit.Wellness;

public sealed class CreateHealthGoalCommandValidatorTests
{
    private readonly CreateHealthGoalCommandValidator _validator = new();

    [Fact]
    public void Validator_accepts_steps_goal_without_unit()
    {
        var result = _validator.TestValidate(new CreateHealthGoalCommand(
            WellnessMetricType.Steps,
            10000m,
            null,
            null));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_requires_unit_and_label_for_custom_metric()
    {
        var result = _validator.TestValidate(new CreateHealthGoalCommand(
            WellnessMetricType.Custom,
            30m,
            null,
            null));

        result.ShouldHaveValidationErrorFor(command => command.Unit);
        result.ShouldHaveValidationErrorFor(command => command.CustomLabel);
    }

    [Fact]
    public void Validator_rejects_non_positive_target()
    {
        var result = _validator.TestValidate(new CreateHealthGoalCommand(
            WellnessMetricType.WaterMl,
            0m,
            "ml",
            null));

        result.ShouldHaveValidationErrorFor(command => command.TargetValue);
    }
}
