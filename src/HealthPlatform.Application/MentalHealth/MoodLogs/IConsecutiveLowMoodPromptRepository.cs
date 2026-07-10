using HealthPlatform.Domain.MentalHealth;

namespace HealthPlatform.Application.MentalHealth.MoodLogs;

public interface IConsecutiveLowMoodPromptRepository
{
    Task<bool> ExistsForTriggeringMoodLogAsync(
        Guid patientId,
        string triggeringMoodLogId,
        CancellationToken ct);

    Task AddAsync(ConsecutiveLowMoodPrompt prompt, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
