namespace HealthPlatform.Domain.Maternal;

public static class FetalMonitoringReminderPolicies
{
    public const int MinIntervalDays = 1;

    public const int MaxIntervalDays = 14;

    public static bool IsValidIntervalDays(int intervalDays) =>
        intervalDays is >= MinIntervalDays and <= MaxIntervalDays;

    public static DateTime CalculateNextReminderAtUtc(int intervalDays, DateTime asOfUtc)
    {
        if (!IsValidIntervalDays(intervalDays))
        {
            throw new ArgumentOutOfRangeException(
                nameof(intervalDays),
                intervalDays,
                $"Fetal monitoring interval must be between {MinIntervalDays} and {MaxIntervalDays} days.");
        }

        if (asOfUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Reference time must be UTC.", nameof(asOfUtc));
        }

        return asOfUtc.AddDays(intervalDays);
    }
}
