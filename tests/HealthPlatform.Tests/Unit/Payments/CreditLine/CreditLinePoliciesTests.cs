using HealthPlatform.Domain.Payments.CreditLine;
using Xunit;

namespace HealthPlatform.Tests.Unit.Payments.CreditLine;

public sealed class CreditLinePoliciesTests
{
    [Theory]
    [InlineData(10000, 10000, true)]
    [InlineData(8000, 10000, false)]
    [InlineData(8001, 10000, true)]
    [InlineData(0, 10000, false)]
    public void ShouldEmitBalanceWarning_uses_eighty_percent_threshold(
        long outstandingMinorUnits,
        long limitMinorUnits,
        bool expected)
    {
        Assert.Equal(expected, CreditLinePolicies.ShouldEmitBalanceWarning(outstandingMinorUnits, limitMinorUnits));
    }
}
