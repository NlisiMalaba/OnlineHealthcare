namespace HealthPlatform.Domain.MentalHealth;

public static class MoodStreakPolicies
{
    public const int ConsecutiveLowMoodPromptThreshold = 3;

    public const int LowMoodRating = 1;

    public static int CountConsecutiveLowRatingsFromMostRecent(IReadOnlyList<int> ratings)
    {
        var count = 0;
        foreach (var rating in ratings)
        {
            if (rating != LowMoodRating)
            {
                break;
            }

            count++;
        }

        return count;
    }
}
