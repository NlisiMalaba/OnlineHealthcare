using FluentValidation.TestHelper;
using HealthPlatform.Application.MentalHealth.CompleteTherapySession;
using Xunit;

namespace HealthPlatform.Tests.Unit.MentalHealth;

public sealed class CompleteTherapySessionCommandValidatorTests
{
    [Fact]
    public void Valid_command_passes_validation()
    {
        var validator = new CompleteTherapySessionCommandValidator();
        var result = validator.TestValidate(new CompleteTherapySessionCommand(
            Guid.CreateVersion7(),
            "Patient demonstrated improved coping strategies."));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_summary_fails_validation()
    {
        var validator = new CompleteTherapySessionCommandValidator();
        var result = validator.TestValidate(new CompleteTherapySessionCommand(
            Guid.CreateVersion7(),
            string.Empty));
        result.ShouldHaveValidationErrorFor(command => command.SessionSummary);
    }
}
