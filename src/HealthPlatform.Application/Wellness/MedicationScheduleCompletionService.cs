using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Wellness.Events;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Wellness;

public sealed class MedicationScheduleCompletionService(
    TimeProvider timeProvider,
    IMedicationScheduleRepository medicationScheduleRepository,
    IAdherenceEventRepository adherenceEventRepository,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    ILogger<MedicationScheduleCompletionService> logger) : IMedicationScheduleCompletionService
{
    public async Task EvaluateCompletionAsync(Guid scheduleId, CancellationToken ct)
    {
        var schedule = await medicationScheduleRepository.GetByIdAsync(scheduleId, ct);
        if (schedule is null || schedule.Status == Domain.Wellness.MedicationScheduleStatus.Completed)
        {
            return;
        }

        var recordedCount = await adherenceEventRepository.CountRecordedByScheduleIdAsync(scheduleId, ct);
        if (recordedCount < schedule.DoseTimes.Count)
        {
            return;
        }

        var completedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        if (!schedule.MarkCompleted(completedAtUtc))
        {
            return;
        }

        await medicationScheduleRepository.UpdateAsync(schedule, ct);

        var domainEvent = new MedicationScheduleCompletedDomainEvent(
            schedule.Id,
            schedule.PrescriptionId,
            schedule.PatientId,
            schedule.MedicationName,
            completedAtUtc);

        await outboxRepository.EnqueueAsync(domainEvent, ct);
        await domainEventPublisher.PublishAsync(domainEvent, ct);

        logger.LogInformation(
            "Medication schedule {ScheduleId} for prescription {PrescriptionId} marked completed.",
            schedule.Id,
            schedule.PrescriptionId);
    }
}
