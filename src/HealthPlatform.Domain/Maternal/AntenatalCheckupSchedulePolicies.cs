namespace HealthPlatform.Domain.Maternal;

public readonly record struct AntenatalCheckupScheduleItem(
    int GestationalAgeWeeks,
    DateOnly RecommendedDate,
    string Description);

public static class AntenatalCheckupSchedulePolicies
{
  private static readonly int[] StandardCheckupWeeks =
  [
      8, 12, 16, 20, 24, 28, 30, 32, 34, 36, 37, 38, 39, 40
  ];

    public const int PregnancyDurationDays = 280;

    public static IReadOnlyList<AntenatalCheckupScheduleItem> BuildRecommendedSchedule(
        int gestationalAgeWeeks,
        DateOnly estimatedDueDate,
        DateOnly asOfDate)
    {
        if (gestationalAgeWeeks < 0 || gestationalAgeWeeks > 42)
        {
            throw new ArgumentOutOfRangeException(
                nameof(gestationalAgeWeeks),
                gestationalAgeWeeks,
                "Gestational age must be between 0 and 42 weeks.");
        }

        var lmp = estimatedDueDate.AddDays(-PregnancyDurationDays);
        var items = new List<AntenatalCheckupScheduleItem>();

        foreach (var week in StandardCheckupWeeks)
        {
            if (week <= gestationalAgeWeeks)
            {
                continue;
            }

            var recommendedDate = lmp.AddDays(week * 7);
            if (recommendedDate < asOfDate)
            {
                continue;
            }

            items.Add(new AntenatalCheckupScheduleItem(
                week,
                recommendedDate,
                $"Recommended antenatal checkup at {week} weeks gestation"));
        }

        return items;
    }
}
