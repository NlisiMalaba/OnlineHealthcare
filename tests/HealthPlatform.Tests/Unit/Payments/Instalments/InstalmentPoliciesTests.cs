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

    [Theory]
    [InlineData(InstalmentFrequency.Weekly, "2026-07-01", "2026-07-08")]
    [InlineData(InstalmentFrequency.Biweekly, "2026-07-01", "2026-07-15")]
    [InlineData(InstalmentFrequency.Monthly, "2026-07-01", "2026-08-01")]
    public void AdvanceDueDate_applies_frequency_offsets(
        InstalmentFrequency frequency,
        string firstDueDate,
        string expectedNextDueDate)
    {
        var first = DateOnly.Parse(firstDueDate, System.Globalization.CultureInfo.InvariantCulture);
        var expected = DateOnly.Parse(expectedNextDueDate, System.Globalization.CultureInfo.InvariantCulture);

        Assert.Equal(expected, InstalmentPolicies.AdvanceDueDate(first, frequency));
    }

    [Fact]
    public void BuildSchedule_rejects_non_positive_total_amount()
    {
        Assert.Throws<InvalidInstalmentPlanException>(() => InstalmentPolicies.BuildSchedule(
            0,
            2,
            InstalmentFrequency.Monthly,
            new DateOnly(2026, 7, 1)));
    }

    [Fact]
    public void IsDueForReminder_is_true_within_24_hour_window_before_due_date()
    {
        var dueDate = new DateOnly(2026, 7, 2);
        var nowUtc = new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc);

        Assert.True(InstalmentPolicies.IsDueForReminder(dueDate, nowUtc));
    }

    [Fact]
    public void IsMissed_is_true_after_grace_period_elapses()
    {
        var dueDate = new DateOnly(2026, 7, 1);
        var nowUtc = new DateTime(2026, 7, 3, 1, 0, 0, DateTimeKind.Utc);

        Assert.True(InstalmentPolicies.IsMissed(dueDate, nowUtc));
    }
}
