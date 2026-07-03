using FluentValidation.TestHelper;
using HealthPlatform.Application.NextOfKin.AddNextOfKinContact;
using Xunit;

namespace HealthPlatform.Tests.Unit.NextOfKin;

public sealed class AddNextOfKinContactCommandValidatorTests
{
    private readonly AddNextOfKinContactCommandValidator _validator = new();

    [Fact]
    public void Validator_accepts_valid_payload()
    {
        var result = _validator.TestValidate(new AddNextOfKinContactCommand(
            "Jane Doe",
            "Sister",
            "+263771234567",
            "jane@example.com",
            true));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_rejects_invalid_phone_number()
    {
        var result = _validator.TestValidate(new AddNextOfKinContactCommand(
            "Jane Doe",
            "Sister",
            "0771234567",
            null,
            false));

        result.ShouldHaveValidationErrorFor(command => command.PhoneNumber);
    }

    [Fact]
    public void Validator_rejects_invalid_email_when_provided()
    {
        var result = _validator.TestValidate(new AddNextOfKinContactCommand(
            "Jane Doe",
            "Sister",
            "+263771234567",
            "not-an-email",
            false));

        result.ShouldHaveValidationErrorFor(command => command.Email);
    }
}
