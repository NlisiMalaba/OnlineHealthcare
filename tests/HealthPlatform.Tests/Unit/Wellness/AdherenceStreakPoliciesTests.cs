using HealthPlatform.Domain.Wellness;
using Xunit;

namespace HealthPlatform.Tests.Unit.Wellness;

public sealed class AdherenceStreakPoliciesTests
{
    [Fact]
    public void CountConsecutiveMissedFromMostRecent_counts_only_leading_missed_events()
    {
        var consecutive = AdherenceStreakPolicies.CountConsecutiveMissedFromMostRecent(
        [
            AdherenceEventStatus.Missed,
            AdherenceEventStatus.Missed,
            AdherenceEventStatus.Taken,
            AdherenceEventStatus.Missed
        ]);

        Assert.Equal(2, consecutive);
    }

    [Theory]
    [InlineData(3, true)]
    [InlineData(2, false)]
    [InlineData(4, false)]
    public void Consecutive_threshold_requires_exactly_three_missed_doses(int consecutiveMissed, bool shouldAlert)
    {
        var alert = consecutiveMissed == AdherenceStreakPolicies.ConsecutiveMissedDoseAlertThreshold;
        Assert.Equal(shouldAlert, alert);
    }
}
