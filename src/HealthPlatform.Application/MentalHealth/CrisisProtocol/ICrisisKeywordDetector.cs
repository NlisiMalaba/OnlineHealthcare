namespace HealthPlatform.Application.MentalHealth.CrisisProtocol;

public interface ICrisisKeywordDetector
{
    bool ContainsCrisisKeyword(string? input);
}
