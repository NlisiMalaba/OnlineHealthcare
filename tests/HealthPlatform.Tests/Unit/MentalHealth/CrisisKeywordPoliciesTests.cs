using HealthPlatform.Domain.MentalHealth;
using Xunit;

namespace HealthPlatform.Tests.Unit.MentalHealth;

public sealed class CrisisKeywordPoliciesTests
{
    [Theory]
    [InlineData("I feel suicidal and hopeless")]
    [InlineData("Sometimes I want to die")]
    [InlineData("I have been self-harming")]
    public void ContainsCrisisKeyword_returns_true_for_crisis_phrases(string input)
    {
        Assert.True(CrisisKeywordPolicies.ContainsCrisisKeyword(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Feeling tired but okay")]
    public void ContainsCrisisKeyword_returns_false_for_non_crisis_input(string? input)
    {
        Assert.False(CrisisKeywordPolicies.ContainsCrisisKeyword(input));
    }
}
