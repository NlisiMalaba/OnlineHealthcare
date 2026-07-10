using HealthPlatform.Domain.Maternal;
using Xunit;

namespace HealthPlatform.Tests.Unit.Maternal;

public sealed class FetalMonitoringReminderPoliciesTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(14)]
    public void IsValidIntervalDays_accepts_clinical_range(int intervalDays)
    {
        Assert.True(FetalMonitoringReminderPolicies.IsValidIntervalDays(intervalDays));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    public void IsValidIntervalDays_rejects_out_of_range_intervals(int intervalDays)
    {
        Assert.False(FetalMonitoringReminderPolicies.IsValidIntervalDays(intervalDays));
    }

    [Fact]
    public void CalculateNextReminderAtUtc_adds_interval_to_reference_time()
    {
        var asOfUtc = new DateTime(2026, 7, 10, 12, 0, 0, DateTimeKind.Utc);

        var nextReminder = FetalMonitoringReminderPolicies.CalculateNextReminderAtUtc(3, asOfUtc);

        Assert.Equal(asOfUtc.AddDays(3), nextReminder);
    }
}
