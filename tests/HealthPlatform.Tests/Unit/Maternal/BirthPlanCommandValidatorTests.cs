using FluentValidation.TestHelper;
using HealthPlatform.Application.Maternal.BirthPlans;
using HealthPlatform.Application.Maternal.BirthPlans.CreateBirthPlan;
using HealthPlatform.Application.Maternal.BirthPlans.GrantMaternalCareAccess;
using Xunit;

namespace HealthPlatform.Tests.Unit.Maternal;

public sealed class BirthPlanCommandValidatorTests
{
    private readonly CreateBirthPlanCommandValidator _createValidator = new();
    private readonly GrantMaternalCareAccessCommandValidator _grantValidator = new();

    [Fact]
    public void Valid_create_command_passes_validation()
    {
        var result = _createValidator.TestValidate(new CreateBirthPlanCommand(
            Guid.CreateVersion7(),
            new BirthPlanContentDto("Natural labour", "Vaginal", "Epidural", "Breastfeeding support")));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_content_fails_validation()
    {
        var result = _createValidator.TestValidate(new CreateBirthPlanCommand(
            Guid.CreateVersion7(),
            new BirthPlanContentDto(null, null, null, null)));

        result.ShouldHaveValidationErrorFor(command => command.Content);
    }

    [Fact]
    public void Grant_requires_at_least_one_shared_resource()
    {
        var result = _grantValidator.TestValidate(new GrantMaternalCareAccessCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            false,
            false));

        result.ShouldHaveValidationErrorFor(command => command);
    }
}
