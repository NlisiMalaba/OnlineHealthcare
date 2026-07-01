namespace HealthPlatform.Domain.Wellness;

public static class AdherenceStreakPolicies
{
    public const int ConsecutiveMissedDoseAlertThreshold = 3;

    public static int CountConsecutiveMissedFromMostRecent(IReadOnlyList<AdherenceEventStatus> statusesNewestFirst)
    {
        var consecutiveMissed = 0;
        foreach (var status in statusesNewestFirst)
        {
            if (status != AdherenceEventStatus.Missed)
            {
                break;
            }

            consecutiveMissed++;
        }

        return consecutiveMissed;
    }
}
