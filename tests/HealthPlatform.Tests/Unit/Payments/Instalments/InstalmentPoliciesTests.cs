using HealthPlatform.Application.Payments.Instalments;
using HealthPlatform.Domain.Payments.Instalments;
using Xunit;

namespace HealthPlatform.Tests.Unit.Payments.Instalments;

public sealed class InstalmentPoliciesTests
{
    [Fact]
    public void BuildSchedule_splits_total_evenly_with_remainder_on_last_instalment()
    {
        var schedule = InstalmentPolicies.BuildSchedule(
            10_003,
            3,
            InstalmentFrequency.Monthly,
            new DateOnly(2026, 7, 1));

        Assert.Equal(3, schedule.Count);
        Assert.Equal(3335, schedule[2].AmountMinorUnits);
        Assert.Equal(10_003, schedule.Sum(entry => entry.AmountMinorUnits));
        Assert.Equal(new DateOnly(2026, 8, 1), schedule[1].DueDate);
    }

    [Theory]
    [InlineData(8_001, 10_000, false)]
    [InlineData(8_000, 10_000, false)]
    [InlineData(10_000, 10_000, true)]
    public void MeetsMinimumExpense_compares_against_threshold(
        long totalMinorUnits,
        long minimumMinorUnits,
        bool expected)
    {
        Assert.Equal(expected, InstalmentPolicies.MeetsMinimumExpense(totalMinorUnits, minimumMinorUnits));
    }
}
