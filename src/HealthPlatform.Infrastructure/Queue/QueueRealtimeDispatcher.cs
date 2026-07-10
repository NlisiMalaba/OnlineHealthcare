using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Queue;
using HealthPlatform.Application.Queue.Realtime;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Queue;

public sealed class QueueRealtimeDispatcher(
    TimeProvider timeProvider,
    IQueueEntryRepository queueEntryRepository,
    IPatientRepository patientRepository,
    IQueueRealtimeNotifier realtimeNotifier,
    IQueuePositionNotifier queuePositionNotifier,
    ILogger<QueueRealtimeDispatcher> logger) : IQueueRealtimeDispatcher
{
    public async Task<int> DispatchAsync(CancellationToken ct)
    {
        var activeEntries = await queueEntryRepository.ListActiveAsync(ct);
        if (activeEntries.Count == 0)
        {
            return 0;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var publishedCount = 0;

        foreach (var entry in activeEntries)
        {
            ct.ThrowIfCancellationRequested();

            await realtimeNotifier.PublishPositionUpdatedAsync(
                new QueuePositionUpdatedRealtimeDto(
                    entry.Id,
                    entry.AppointmentId,
                    entry.QueuePosition,
                    entry.EstimatedWaitMinutes,
                    now),
                ct);
            publishedCount++;

            if (entry.QueuePosition != 2 || !entry.MarkPositionTwoNotified(now))
            {
                continue;
            }

            var patient = await patientRepository.GetByIdAsync(entry.PatientId, ct);
            if (patient is null)
            {
                logger.LogWarning(
                    "Skipping queue position notification for queue entry {QueueEntryId}; patient {PatientId} was not found.",
                    entry.Id,
                    entry.PatientId);
                continue;
            }

            await queuePositionNotifier.NotifySecondPositionReachedAsync(
                patient.UserId,
                entry.Id,
                entry.AppointmentId,
                entry.EstimatedWaitMinutes,
                ct);

            await queueEntryRepository.UpdateAsync(entry, ct);
        }

        return publishedCount;
    }
}
