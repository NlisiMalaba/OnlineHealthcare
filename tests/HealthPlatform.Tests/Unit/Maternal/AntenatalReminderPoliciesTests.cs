using HealthPlatform.Domain.Maternal;
using Xunit;

namespace HealthPlatform.Tests.Unit.Maternal;

public sealed class AntenatalReminderPoliciesTests
{
    [Fact]
    public void GetReminderIntervalDays_uses_high_frequency_within_four_weeks_of_due_date()
    {
        var dueDate = new DateOnly(2026, 8, 1);
        var asOfDate = new DateOnly(2026, 7, 10);

        var intervalDays = AntenatalReminderPolicies.GetReminderIntervalDays(dueDate, asOfDate);

        Assert.Equal(AntenatalReminderPolicies.HighFrequencyReminderIntervalDays, intervalDays);
        Assert.True(AntenatalReminderPolicies.IsWithinDueDateProximity(dueDate, asOfDate));
    }

    [Fact]
    public void GetReminderIntervalDays_uses_standard_frequency_when_due_date_is_farther_than_four_weeks()
    {
        var dueDate = new DateOnly(2026, 12, 1);
        var asOfDate = new DateOnly(2026, 7, 10);

        var intervalDays = AntenatalReminderPolicies.GetReminderIntervalDays(dueDate, asOfDate);

        Assert.Equal(AntenatalReminderPolicies.StandardReminderIntervalDays, intervalDays);
        Assert.False(AntenatalReminderPolicies.IsWithinDueDateProximity(dueDate, asOfDate));
    }

    [Fact]
    public void IsWithinDueDateProximity_is_true_at_exactly_twenty_eight_days_before_due_date()
    {
        var dueDate = new DateOnly(2026, 8, 7);
        var asOfDate = new DateOnly(2026, 7, 10);

        Assert.True(AntenatalReminderPolicies.IsWithinDueDateProximity(dueDate, asOfDate));
        Assert.Equal(
            AntenatalReminderPolicies.HighFrequencyReminderIntervalDays,
            AntenatalReminderPolicies.GetReminderIntervalDays(dueDate, asOfDate));
    }

    [Fact]
    public void CalculateNextReminderAtUtc_escalates_to_high_frequency_interval_within_proximity_window()
    {
        var dueDate = new DateOnly(2026, 8, 1);
        var asOfUtc = new DateTime(2026, 7, 10, 9, 0, 0, DateTimeKind.Utc);

        var nextReminder = AntenatalReminderPolicies.CalculateNextReminderAtUtc(dueDate, asOfUtc);

        Assert.Equal(
            asOfUtc.AddDays(AntenatalReminderPolicies.HighFrequencyReminderIntervalDays),
            nextReminder);
    }
}
