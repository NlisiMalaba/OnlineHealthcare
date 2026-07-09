using HealthPlatform.Domain.Queue;

namespace HealthPlatform.Application.Queue;

public static class QueueProjectionUpdater
{
    public static void Recalculate(IReadOnlyList<QueueEntry> entries, int averageConsultationDurationMinutes)
    {
        for (var index = 0; index < entries.Count; index++)
        {
            var queuePosition = index + 1;
            var estimatedWaitMinutes = index * averageConsultationDurationMinutes;
            entries[index].SetQueueProjection(queuePosition, estimatedWaitMinutes);
        }
    }
}
