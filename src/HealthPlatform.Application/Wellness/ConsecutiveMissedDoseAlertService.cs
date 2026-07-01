using HealthPlatform.Application.NextOfKin;using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Wellness;
using HealthPlatform.Domain.Wellness.Events;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Wellness;

public sealed class ConsecutiveMissedDoseAlertService(
    TimeProvider timeProvider,
    IAdherenceEventRepository adherenceEventRepository,
    IConsecutiveMissedDoseAlertRepository consecutiveMissedDoseAlertRepository,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    ILogger<ConsecutiveMissedDoseAlertService> logger) : IConsecutiveMissedDoseAlertService
{
    public async Task TryEmitAlertIfThresholdReachedAsync(Guid patientId, CancellationToken ct)
    {
        var recentEvents = await adherenceEventRepository.ListByPatientIdOrderedByScheduledDescAsync(
            patientId,
            WellnessPolicies.AdherenceStreakLookbackCount,
            ct);

        if (recentEvents.Count < AdherenceStreakPolicies.ConsecutiveMissedDoseAlertThreshold)
        {
            return;
        }

        var consecutiveMissed = AdherenceStreakPolicies.CountConsecutiveMissedFromMostRecent(
            recentEvents.Select(adherenceEvent => adherenceEvent.Status).ToList());

        if (consecutiveMissed != AdherenceStreakPolicies.ConsecutiveMissedDoseAlertThreshold)
        {
            return;
        }

        var triggeringEvent = recentEvents[0];
        if (triggeringEvent.Status != AdherenceEventStatus.Missed)
        {
            return;
        }

        if (await consecutiveMissedDoseAlertRepository.ExistsForStreakAsync(
                patientId,
                triggeringEvent.ScheduledAtUtc,
                ct))
        {
            return;
        }

        var triggeredAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var domainEvent = new ConsecutiveMissedDosesDetectedDomainEvent(
            patientId,
            triggeringEvent.Id,
            triggeringEvent.ScheduledAtUtc);

        await outboxRepository.EnqueueAsync(domainEvent, ct);
        await domainEventPublisher.PublishAsync(domainEvent, ct);

        var alertRecord = ConsecutiveMissedDoseAlert.Record(
            patientId,
            triggeringEvent.Id,
            triggeringEvent.ScheduledAtUtc,
            triggeredAtUtc);

        await consecutiveMissedDoseAlertRepository.AddAsync(alertRecord, ct);
        await consecutiveMissedDoseAlertRepository.SaveChangesAsync(ct);

        logger.LogInformation(
            "Emitted consecutive missed dose alert for patient {PatientId} after adherence event {AdherenceEventId}.",
            patientId,
            triggeringEvent.Id);
    }
}
