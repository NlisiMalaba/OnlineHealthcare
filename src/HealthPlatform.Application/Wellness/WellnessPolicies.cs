namespace HealthPlatform.Application.Wellness;

public static class WellnessPolicies
{
    /// <summary>
    /// How far back each scheduler tick searches for dose times that have been reached but not yet reminded.
    /// Should be at least the Hangfire job interval to avoid missing reminders.
    /// </summary>
    public static readonly TimeSpan DoseReminderLookbackWindow = TimeSpan.FromMinutes(5);

    public const int DoseReminderBatchSize = 100;

    public static readonly TimeSpan MissedDoseGracePeriod = TimeSpan.FromHours(2);

    public const int MissedDoseDetectionBatchSize = 100;

    public static bool IsMissed(DateTime scheduledAtUtc, DateTime nowUtc) =>
        nowUtc >= scheduledAtUtc.Add(MissedDoseGracePeriod);

    public static bool CanConfirmDose(DateTime scheduledAtUtc, DateTime nowUtc) =>
        nowUtc >= scheduledAtUtc && nowUtc < scheduledAtUtc.Add(MissedDoseGracePeriod);
}
