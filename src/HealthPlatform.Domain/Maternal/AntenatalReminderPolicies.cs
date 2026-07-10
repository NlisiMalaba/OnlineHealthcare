namespace HealthPlatform.Domain.Maternal;

public static class AntenatalReminderPolicies
{
    public const int DueDateProximityThresholdDays = 28;

    public const int StandardReminderIntervalDays = 14;

    public const int HighFrequencyReminderIntervalDays = 3;

    public static bool IsWithinDueDateProximity(DateOnly estimatedDueDate, DateOnly asOfDate) =>
        estimatedDueDate.DayNumber - asOfDate.DayNumber <= DueDateProximityThresholdDays;

    public static int GetReminderIntervalDays(DateOnly estimatedDueDate, DateOnly asOfDate) =>
        IsWithinDueDateProximity(estimatedDueDate, asOfDate)
            ? HighFrequencyReminderIntervalDays
            : StandardReminderIntervalDays;

    public static DateTime CalculateNextReminderAtUtc(DateOnly estimatedDueDate, DateTime asOfUtc)
    {
        if (asOfUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Reference time must be UTC.", nameof(asOfUtc));
        }

        var intervalDays = GetReminderIntervalDays(estimatedDueDate, DateOnly.FromDateTime(asOfUtc));
        return asOfUtc.AddDays(intervalDays);
    }
}
