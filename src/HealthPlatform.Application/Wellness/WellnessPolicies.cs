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

    public const int AdherenceStreakLookbackCount = 10;

    public static readonly TimeSpan AdherenceSummaryWeeklyWindow = TimeSpan.FromDays(7);

    public static readonly TimeSpan AdherenceSummaryMonthlyWindow = TimeSpan.FromDays(30);

    public static bool IsMissed(DateTime scheduledAtUtc, DateTime nowUtc) =>
        nowUtc >= scheduledAtUtc.Add(MissedDoseGracePeriod);

    public static bool CanConfirmDose(DateTime scheduledAtUtc, DateTime nowUtc) =>
        nowUtc >= scheduledAtUtc && nowUtc < scheduledAtUtc.Add(MissedDoseGracePeriod);

    public const int MaxUnitLength = 32;

    public const int MaxCustomLabelLength = 100;

    public const decimal MinTargetValue = 0.01m;

    public const decimal MaxTargetValue = 1_000_000m;

    public const decimal MaxEntryValue = 1_000_000m;

    public static readonly TimeSpan WellnessChartDefaultLookback = TimeSpan.FromDays(30);

    public const string DefaultStepsUnit = "steps";

    public const string DefaultWeightUnit = "kg";

    public const string DefaultSleepHoursUnit = "hours";

    public const string DefaultWaterMlUnit = "ml";
}
