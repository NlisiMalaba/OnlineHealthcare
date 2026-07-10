namespace HealthPlatform.Application.MentalHealth.CrisisProtocol;

public interface ICrisisProtocolService
{
    Task<CrisisProtocolDto> TryTriggerAsync(
        Guid patientId,
        string? inputText,
        CrisisProtocolInputSource inputSource,
        CancellationToken ct);
}
