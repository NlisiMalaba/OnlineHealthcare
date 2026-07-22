using FluentValidation.TestHelper;
using HealthPlatform.Application.Wellness.WellnessEntries.RecordWellnessEntry;
using HealthPlatform.Domain.Wellness;
using Xunit;

namespace HealthPlatform.Tests.Unit.Wellness;

public sealed class RecordWellnessEntryCommandValidatorTests
{
    private readonly RecordWellnessEntryCommandValidator _validator = new();

    [Fact]
    public void Validator_accepts_valid_entry()
    {
        var result = _validator.TestValidate(new RecordWellnessEntryCommand(
            WellnessMetricType.Steps,
            7500m,
            null,
            DateTime.UtcNow));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_rejects_negative_value()
    {
        var result = _validator.TestValidate(new RecordWellnessEntryCommand(
            WellnessMetricType.SleepHours,
            -1m,
            null,
            null));

        result.ShouldHaveValidationErrorFor(command => command.Value);
    }

    [Fact]
    public void Validator_rejects_empty_goal_id()
    {
        var result = _validator.TestValidate(new RecordWellnessEntryCommand(
            WellnessMetricType.Weight,
            72m,
            Guid.Empty,
            null));

        result.ShouldHaveValidationErrorFor(command => command.GoalId);
    }
}
