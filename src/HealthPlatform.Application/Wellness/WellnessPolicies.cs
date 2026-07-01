namespace HealthPlatform.Application.Wellness;

public static class WellnessPolicies
{
    /// <summary>
    /// How far back each scheduler tick searches for dose times that have been reached but not yet reminded.
    /// Should be at least the Hangfire job interval to avoid missing reminders.
    /// </summary>
    public static readonly TimeSpan DoseReminderLookbackWindow = TimeSpan.FromMinutes(5);

    public const int DoseReminderBatchSize = 100;
}
