using HealthPlatform.Domain.MentalHealth;

namespace HealthPlatform.Application.MentalHealth.CrisisProtocol;

public sealed class CrisisKeywordDetector : ICrisisKeywordDetector
{
    public bool ContainsCrisisKeyword(string? input) =>
        CrisisKeywordPolicies.ContainsCrisisKeyword(input);
}
