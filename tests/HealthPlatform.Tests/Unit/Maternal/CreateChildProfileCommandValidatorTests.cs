using FluentValidation.TestHelper;
using HealthPlatform.Application.Maternal.ChildProfiles.CreateChildProfile;
using Xunit;

namespace HealthPlatform.Tests.Unit.Maternal;

public sealed class CreateChildProfileCommandValidatorTests
{
    private readonly CreateChildProfileCommandValidator _validator = new();

    [Fact]
    public void Valid_command_passes_validation()
    {
        var result = _validator.TestValidate(new CreateChildProfileCommand(
            "Child Name",
            DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
            "B+",
            ["Eggs"]));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_full_name_fails_validation()
    {
        var result = _validator.TestValidate(new CreateChildProfileCommand(
            "",
            DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
            null,
            []));

        result.ShouldHaveValidationErrorFor(command => command.FullName);
    }
}
