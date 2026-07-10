namespace HealthPlatform.Application.MentalHealth.CrisisProtocol;

public sealed record CrisisHelplineDto(string Name, string PhoneNumber, string? WebsiteUrl);

public sealed record CrisisProtocolDto(
    bool Triggered,
    string EmergencyServicesPrompt,
    IReadOnlyList<CrisisHelplineDto> Helplines,
    int MentalHealthContactsNotified)
{
    public static CrisisProtocolDto NotTriggered() =>
        new(false, string.Empty, [], 0);
}
