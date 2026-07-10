using HealthPlatform.Domain.Queue;

namespace HealthPlatform.Application.Queue;

public static class QueueProjectionUpdater
{
    public static void Recalculate(IReadOnlyList<QueueEntry> entries, int averageConsultationDurationMinutes)
    {
        Recalculate(entries, averageConsultationDurationMinutes, 0);
    }

    public static void Recalculate(
        IReadOnlyList<QueueEntry> entries,
        int averageConsultationDurationMinutes,
        int delayMinutes)
    {
        if (delayMinutes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(delayMinutes));
        }

        for (var index = 0; index < entries.Count; index++)
        {
            var queuePosition = index + 1;
            var estimatedWaitMinutes = (index * averageConsultationDurationMinutes) + delayMinutes;
            entries[index].SetQueueProjection(queuePosition, estimatedWaitMinutes);
        }
    }
}
