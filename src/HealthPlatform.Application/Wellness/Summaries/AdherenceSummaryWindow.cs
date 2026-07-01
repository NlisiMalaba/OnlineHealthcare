namespace HealthPlatform.Application.Wellness.Summaries;

public static class AdherenceSummaryWindow
{
    public static (DateTime FromUtc, DateTime ToUtc) Resolve(AdherenceSummaryPeriod period, DateTime nowUtc)
    {
        var window = period switch
        {
            AdherenceSummaryPeriod.Weekly => WellnessPolicies.AdherenceSummaryWeeklyWindow,
            AdherenceSummaryPeriod.Monthly => WellnessPolicies.AdherenceSummaryMonthlyWindow,
            _ => throw new ArgumentOutOfRangeException(nameof(period), period, "Unsupported adherence summary period.")
        };

        return (nowUtc.Subtract(window), nowUtc);
    }
}
