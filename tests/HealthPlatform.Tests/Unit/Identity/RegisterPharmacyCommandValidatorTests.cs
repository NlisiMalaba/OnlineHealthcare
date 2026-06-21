using FluentValidation.TestHelper;
using HealthPlatform.Application.Identity.RegisterPharmacy;
using HealthPlatform.Application.Identity.UpdatePharmacyProfile;
using Xunit;

namespace HealthPlatform.Tests.Unit.Identity;

public sealed class RegisterPharmacyCommandValidatorTests
{
    private readonly RegisterPharmacyCommandValidator _registerValidator = new();
    private readonly UpdatePharmacyProfileCommandValidator _updateValidator = new();

    [Fact]
    public void Valid_registration_passes()
    {
        var result = _registerValidator.TestValidate(new RegisterPharmacyCommand(
            "Central Pharmacy",
            "12 Samora Machel Ave, Harare",
            -17.8292,
            31.0522,
            "pharmacy@example.com",
            "+263771234567",
            "ValidPassw0rd!12",
            null));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_profile_update_is_rejected()
    {
        var result = _updateValidator.TestValidate(new UpdatePharmacyProfileCommand(
            null,
            null,
            null,
            null,
            null,
            null));

        result.ShouldHaveValidationErrorFor(x => x);
    }
}
