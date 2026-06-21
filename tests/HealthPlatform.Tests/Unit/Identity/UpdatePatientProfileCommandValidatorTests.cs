using HealthPlatform.Application.Identity.UpdatePatientProfile;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Xunit;

namespace HealthPlatform.Tests.Unit.Identity;

public sealed class UpdatePatientProfileCommandValidatorTests
{
    private readonly UpdatePatientProfileCommandValidator _validator = new();

    [Fact]
    public void Update_WithNoFields_FailsValidation()
    {
        var command = new UpdatePatientProfileCommand(null, null, null, null, null, null);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Update_WithValidFields_PassesValidation()
    {
        var command = new UpdatePatientProfileCommand(
            "Updated Name",
            new DateOnly(1990, 5, 15),
            BloodType.OPositive,
            ["Peanuts"],
            ["Asthma"],
            null);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Update_WithFutureDateOfBirth_FailsValidation()
    {
        var command = new UpdatePatientProfileCommand(
            null,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            null,
            null,
            null,
            null);

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdatePatientProfileCommand.DateOfBirth));
    }
}
