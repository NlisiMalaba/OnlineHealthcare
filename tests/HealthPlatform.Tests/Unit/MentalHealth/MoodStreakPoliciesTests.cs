using HealthPlatform.Domain.MentalHealth;
using Xunit;

namespace HealthPlatform.Tests.Unit.MentalHealth;

public sealed class MoodStreakPoliciesTests
{
    [Fact]
    public void CountConsecutiveLowRatingsFromMostRecent_counts_only_leading_low_ratings()
    {
        var consecutive = MoodStreakPolicies.CountConsecutiveLowRatingsFromMostRecent(
        [
            MoodStreakPolicies.LowMoodRating,
            MoodStreakPolicies.LowMoodRating,
            3,
            MoodStreakPolicies.LowMoodRating
        ]);

        Assert.Equal(2, consecutive);
    }

    [Theory]
    [InlineData(3, true)]
    [InlineData(2, false)]
    [InlineData(4, false)]
    public void Consecutive_threshold_requires_exactly_three_low_ratings(int consecutiveLowRatings, bool shouldPrompt)
    {
        var prompt = consecutiveLowRatings == MoodStreakPolicies.ConsecutiveLowMoodPromptThreshold;
        Assert.Equal(shouldPrompt, prompt);
    }
}
