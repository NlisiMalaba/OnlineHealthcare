using FsCheck;
using HealthPlatform.Domain.MentalHealth;

namespace HealthPlatform.Tests.Arbitraries;

public enum ConsecutiveLowMoodPromptExpectation
{
    ShouldPrompt = 0,
    ShouldNotPrompt = 1
}

public sealed record ConsecutiveLowMoodPromptCase(
    ConsecutiveLowMoodPromptExpectation Expectation,
    int PrefixRatingCount,
    int TrailingLowCount);

public static class MentalHealthArbitraries
{
    public static Arbitrary<ConsecutiveLowMoodPromptCase> ConsecutiveLowMoodPromptCase() =>
        Gen.OneOf(ShouldPromptCase(), ShouldNotPromptCase()).ToArbitrary();

    private static Gen<ConsecutiveLowMoodPromptCase> ShouldPromptCase() =>
        from prefixCount in Gen.Choose(0, 4)
        select new ConsecutiveLowMoodPromptCase(
            ConsecutiveLowMoodPromptExpectation.ShouldPrompt,
            prefixCount,
            MoodStreakPolicies.ConsecutiveLowMoodPromptThreshold);

    private static Gen<ConsecutiveLowMoodPromptCase> ShouldNotPromptCase() =>
        from prefixCount in Gen.Choose(0, 4)
        from trailingLowCount in Gen.Choose(1, MoodStreakPolicies.ConsecutiveLowMoodPromptThreshold - 1)
        select new ConsecutiveLowMoodPromptCase(
            ConsecutiveLowMoodPromptExpectation.ShouldNotPrompt,
            prefixCount,
            trailingLowCount);
}
