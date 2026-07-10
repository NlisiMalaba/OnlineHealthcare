namespace HealthPlatform.Domain.MentalHealth;

public static class CrisisKeywordPolicies
{
    public static readonly string[] Keywords =
    [
        "suicide",
        "suicidal",
        "kill myself",
        "end my life",
        "self-harm",
        "self harm",
        "want to die",
        "hurt myself",
        "can't go on",
        "cannot go on",
        "no reason to live"
    ];

    public static bool ContainsCrisisKeyword(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var normalized = input.Trim().ToLowerInvariant();
        return Keywords.Any(keyword => normalized.Contains(keyword, StringComparison.Ordinal));
    }
}
