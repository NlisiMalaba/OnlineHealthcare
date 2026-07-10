namespace HealthPlatform.Domain.Queue;

public static class QueueWaitTimeCalculator
{
    public static int ComputeQueuePosition(int activeEntryCount)
    {
        if (activeEntryCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(activeEntryCount));
        }

        return activeEntryCount + 1;
    }

    public static int ComputeEstimatedWaitMinutes(int patientsAhead, int averageConsultationDurationMinutes)
    {
        if (patientsAhead < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(patientsAhead));
        }

        if (averageConsultationDurationMinutes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(averageConsultationDurationMinutes));
        }

        return patientsAhead * averageConsultationDurationMinutes;
    }
}
