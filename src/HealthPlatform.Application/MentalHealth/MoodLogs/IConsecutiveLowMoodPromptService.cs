namespace HealthPlatform.Application.MentalHealth.MoodLogs;

public interface IConsecutiveLowMoodPromptService
{
    Task TryEmitPromptIfThresholdReachedAsync(Guid patientId, CancellationToken ct);
}
