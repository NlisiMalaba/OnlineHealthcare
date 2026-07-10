using FluentValidation.TestHelper;
using HealthPlatform.Application.MentalHealth.CrisisProtocol;
using HealthPlatform.Application.MentalHealth.CrisisProtocol.EvaluateCrisisInput;
using Xunit;

namespace HealthPlatform.Tests.Unit.MentalHealth;

public sealed class EvaluateCrisisInputCommandValidatorTests
{
    private readonly EvaluateCrisisInputCommandValidator _validator = new();

    [Fact]
    public void Should_fail_when_input_text_is_empty()
    {
        var result = _validator.TestValidate(new EvaluateCrisisInputCommand(string.Empty));
        result.ShouldHaveValidationErrorFor(command => command.InputText)
            .WithErrorCode(CrisisProtocolErrorCodes.InputTextRequired);
    }

    [Fact]
    public void Should_fail_when_input_text_exceeds_max_length()
    {
        var result = _validator.TestValidate(new EvaluateCrisisInputCommand(new string('a', 4001)));
        result.ShouldHaveValidationErrorFor(command => command.InputText)
            .WithErrorCode(CrisisProtocolErrorCodes.InputTextTooLong);
    }

    [Fact]
    public void Should_pass_for_valid_input_text()
    {
        var result = _validator.TestValidate(new EvaluateCrisisInputCommand("I need help"));
        result.ShouldNotHaveAnyValidationErrors();
    }
}
