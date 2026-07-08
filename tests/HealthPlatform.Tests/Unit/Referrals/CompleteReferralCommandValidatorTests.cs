using HealthPlatform.Application.Referrals.CompleteReferral;
using Xunit;

namespace HealthPlatform.Tests.Unit.Referrals;

public sealed class CompleteReferralCommandValidatorTests
{
    private readonly CompleteReferralCommandValidator _validator = new();

    [Fact]
    public void Valid_command_passes_validation()
    {
        var result = _validator.Validate(new CompleteReferralCommand(
            Guid.CreateVersion7(),
            "Patient reviewed and treatment plan updated."));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Missing_summary_fails_validation()
    {
        var result = _validator.Validate(new CompleteReferralCommand(
            Guid.CreateVersion7(),
            ""));

        Assert.False(result.IsValid);
    }
}
