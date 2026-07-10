using FluentValidation.TestHelper;
using HealthPlatform.Application.MentalHealth.MoodLogs.CreateMoodLog;
using Xunit;

namespace HealthPlatform.Tests.Unit.MentalHealth;

public sealed class CreateMoodLogCommandValidatorTests
{
    [Fact]
    public void Valid_command_passes_validation()
    {
        var validator = new CreateMoodLogCommandValidator();
        var result = validator.TestValidate(new CreateMoodLogCommand(4, "Feeling okay", DateTime.UtcNow));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Invalid_rating_fails_validation(int rating)
    {
        var validator = new CreateMoodLogCommandValidator();
        var result = validator.TestValidate(new CreateMoodLogCommand(rating, null, null));
        result.ShouldHaveValidationErrorFor(command => command.Rating);
    }
}
