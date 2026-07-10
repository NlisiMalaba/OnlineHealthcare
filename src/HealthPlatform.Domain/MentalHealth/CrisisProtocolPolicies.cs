namespace HealthPlatform.Domain.MentalHealth;

public sealed record CrisisHelpline(string Name, string PhoneNumber, string? WebsiteUrl);

public static class CrisisProtocolPolicies
{
    public const string EmergencyServicesPrompt =
        "If you are in immediate danger, contact your local emergency services now.";

    public static readonly IReadOnlyList<CrisisHelpline> DefaultHelplines =
    [
        new CrisisHelpline("988 Suicide and Crisis Lifeline", "988", "https://988lifeline.org"),
        new CrisisHelpline("Crisis Text Line", "741741", "https://www.crisistextline.org"),
        new CrisisHelpline("Emergency Services", "112", null)
    ];
}
