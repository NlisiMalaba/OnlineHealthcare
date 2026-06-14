using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Xunit;

namespace HealthPlatform.Tests.Unit.Identity;

public sealed class RegisterPatientCommandValidatorTests
{
    private readonly RegisterPatientCommandValidator _validator = new();

    [Fact]
    public void PhoneRegistration_WithValidInput_PassesValidation()
    {
        var command = new RegisterPatientCommand(
            PatientAuthProvider.Phone,
            "Jane Doe",
            "+263771234567",
            null,
            PatientRegistrationTestHost.ValidPassword,
            null);

        var result = _validator.Validate(command);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void PhoneRegistration_WithInvalidPhone_FailsValidation()
    {
        var command = new RegisterPatientCommand(
            PatientAuthProvider.Phone,
            "Jane Doe",
            "0771234567",
            null,
            PatientRegistrationTestHost.ValidPassword,
            null);

        var result = _validator.Validate(command);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterPatientCommand.PhoneNumber));
    }

    [Fact]
    public void EmailRegistration_WithoutPassword_FailsValidation()
    {
        var command = new RegisterPatientCommand(
            PatientAuthProvider.Email,
            "Jane Doe",
            null,
            "jane@example.com",
            null,
            null);

        var result = _validator.Validate(command);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterPatientCommand.Password));
    }

    [Fact]
    public void GoogleRegistration_WithoutIdToken_FailsValidation()
    {
        var command = new RegisterPatientCommand(
            PatientAuthProvider.Google,
            "Jane Doe",
            null,
            null,
            null,
            null);

        var result = _validator.Validate(command);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterPatientCommand.IdToken));
    }
}
