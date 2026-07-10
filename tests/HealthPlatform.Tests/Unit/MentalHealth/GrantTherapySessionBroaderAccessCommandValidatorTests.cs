using FluentValidation.TestHelper;
using HealthPlatform.Application.MentalHealth.GrantTherapySessionBroaderAccess;
using Xunit;

namespace HealthPlatform.Tests.Unit.MentalHealth;

public sealed class GrantTherapySessionBroaderAccessCommandValidatorTests
{
    [Fact]
    public void Valid_command_passes_validation()
    {
        var validator = new GrantTherapySessionBroaderAccessCommandValidator();
        var result = validator.TestValidate(new GrantTherapySessionBroaderAccessCommand(Guid.CreateVersion7()));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_session_id_fails_validation()
    {
        var validator = new GrantTherapySessionBroaderAccessCommandValidator();
        var result = validator.TestValidate(new GrantTherapySessionBroaderAccessCommand(Guid.Empty));
        result.ShouldHaveValidationErrorFor(command => command.TherapySessionId);
    }
}
