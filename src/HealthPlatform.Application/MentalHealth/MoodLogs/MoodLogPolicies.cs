namespace HealthPlatform.Application.MentalHealth.MoodLogs;

public static class MoodLogPolicies
{
    public const int MinRating = 1;
    public const int MaxRating = 5;
    public const int MaxNotesLength = 2000;
    public static readonly TimeSpan DefaultChartWindow = TimeSpan.FromDays(90);

    public static bool IsValidRating(int rating) => rating is >= MinRating and <= MaxRating;
}
