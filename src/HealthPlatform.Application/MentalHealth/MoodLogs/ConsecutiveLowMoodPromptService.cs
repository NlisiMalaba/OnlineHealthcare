using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.MentalHealth;
using HealthPlatform.Domain.MentalHealth.Events;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.MentalHealth.MoodLogs;

public sealed class ConsecutiveLowMoodPromptService(
    TimeProvider timeProvider,
    IMoodLogRepository moodLogRepository,
    IConsecutiveLowMoodPromptRepository consecutiveLowMoodPromptRepository,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    ILogger<ConsecutiveLowMoodPromptService> logger) : IConsecutiveLowMoodPromptService
{
    public async Task TryEmitPromptIfThresholdReachedAsync(Guid patientId, CancellationToken ct)
    {
        var recentLogs = await moodLogRepository.ListRecentByPatientIdAsync(
            patientId,
            MoodStreakPolicies.ConsecutiveLowMoodPromptThreshold,
            ct);

        if (recentLogs.Count < MoodStreakPolicies.ConsecutiveLowMoodPromptThreshold)
        {
            return;
        }

        var consecutiveLowRatings = MoodStreakPolicies.CountConsecutiveLowRatingsFromMostRecent(
            recentLogs.Select(log => log.Rating).ToList());

        if (consecutiveLowRatings != MoodStreakPolicies.ConsecutiveLowMoodPromptThreshold)
        {
            return;
        }

        var triggeringLog = recentLogs[0];
        if (triggeringLog.Rating != MoodStreakPolicies.LowMoodRating)
        {
            return;
        }

        if (await consecutiveLowMoodPromptRepository.ExistsForTriggeringMoodLogAsync(
                patientId,
                triggeringLog.Id,
                ct))
        {
            return;
        }

        var triggeredAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var domainEvent = new ConsecutiveLowMoodDetectedDomainEvent(
            patientId,
            triggeringLog.Id,
            triggeringLog.LoggedAtUtc);

        await outboxRepository.EnqueueAsync(domainEvent, ct);
        await domainEventPublisher.PublishAsync(domainEvent, ct);

        var promptRecord = ConsecutiveLowMoodPrompt.Record(
            patientId,
            triggeringLog.Id,
            triggeringLog.LoggedAtUtc,
            triggeredAtUtc);

        await consecutiveLowMoodPromptRepository.AddAsync(promptRecord, ct);
        await consecutiveLowMoodPromptRepository.SaveChangesAsync(ct);

        logger.LogInformation(
            "Emitted consecutive low mood prompt for patient {PatientId} after mood log {MoodLogId}.",
            patientId,
            triggeringLog.Id);
    }
}
