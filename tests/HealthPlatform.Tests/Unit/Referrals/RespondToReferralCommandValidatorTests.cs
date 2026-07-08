using HealthPlatform.Application.Referrals.RespondToReferral;
using HealthPlatform.Domain.Referrals;
using Xunit;

namespace HealthPlatform.Tests.Unit.Referrals;

public sealed class RespondToReferralCommandValidatorTests
{
    private readonly RespondToReferralCommandValidator _validator = new();

    [Fact]
    public void Accept_without_reason_is_valid()
    {
        var result = _validator.Validate(new RespondToReferralCommand(
            Guid.CreateVersion7(),
            ReferralResponseAction.Accept,
            null));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Decline_requires_reason()
    {
        var result = _validator.Validate(new RespondToReferralCommand(
            Guid.CreateVersion7(),
            ReferralResponseAction.Decline,
            null));

        Assert.False(result.IsValid);
    }
}
