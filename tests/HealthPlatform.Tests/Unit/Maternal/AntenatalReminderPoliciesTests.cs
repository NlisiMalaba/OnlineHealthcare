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
}
