using HealthPlatform.Application.Referrals.CreateReferral;
using Xunit;

namespace HealthPlatform.Tests.Unit.Referrals;

public sealed class CreateReferralCommandValidatorTests
{
    private readonly CreateReferralCommandValidator _validator = new();

    [Fact]
    public void Valid_command_passes_validation()
    {
        var result = _validator.Validate(new CreateReferralCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            null,
            "Further specialist investigation required.",
            "Attach previous treatment notes.",
            ["diagnoses", "medications"],
            DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Missing_patient_consent_fails_validation()
    {
        var result = _validator.Validate(new CreateReferralCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            null,
            "Further specialist investigation required.",
            null,
            ["diagnoses"],
            default));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Missing_receiving_target_fails_validation()
    {
        var result = _validator.Validate(new CreateReferralCommand(
            Guid.CreateVersion7(),
            null,
            null,
            "Further specialist investigation required.",
            null,
            ["diagnoses"],
            DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)));

        Assert.False(result.IsValid);
    }
}
