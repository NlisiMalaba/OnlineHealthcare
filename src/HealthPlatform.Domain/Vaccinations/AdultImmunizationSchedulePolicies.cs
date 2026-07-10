namespace HealthPlatform.Domain.Vaccinations;

public readonly record struct AdultImmunizationScheduleItem(
    string VaccineName,
    DateOnly RecommendedDate,
    string Description);

public static class AdultImmunizationSchedulePolicies
{
    private const int InfluenzaMinimumAgeYears = 18;
    private const int ShinglesMinimumAgeYears = 50;
    private const int PneumococcalMinimumAgeYears = 65;

    public static IReadOnlyList<AdultImmunizationScheduleItem> BuildRecommendedSchedule(
        DateOnly? dateOfBirth,
        IReadOnlyCollection<string> chronicConditions,
        DateOnly asOfDate)
    {
        if (dateOfBirth is null || dateOfBirth > asOfDate)
        {
            return [];
        }

        var ageYears = CalculateAgeYears(dateOfBirth.Value, asOfDate);
        var hasChronicCondition = chronicConditions.Count > 0;
        var items = new List<AdultImmunizationScheduleItem>();

        if (ageYears >= InfluenzaMinimumAgeYears && (ageYears >= PneumococcalMinimumAgeYears || hasChronicCondition))
        {
            items.Add(new AdultImmunizationScheduleItem(
                "Influenza",
                asOfDate,
                hasChronicCondition
                    ? "Annual influenza vaccination for high-risk adults"
                    : "Annual influenza vaccination"));
        }
        else if (ageYears >= InfluenzaMinimumAgeYears)
        {
            items.Add(new AdultImmunizationScheduleItem(
                "Influenza",
                asOfDate,
                "Annual influenza vaccination"));
        }

        if (ageYears >= 18)
        {
            items.Add(new AdultImmunizationScheduleItem(
                "Tdap",
                asOfDate,
                "Tetanus, diphtheria, and pertussis booster"));
        }

        if (ageYears >= ShinglesMinimumAgeYears)
        {
            items.Add(new AdultImmunizationScheduleItem(
                "Shingles",
                asOfDate,
                "Shingles (herpes zoster) vaccination"));
        }

        if (ageYears >= PneumococcalMinimumAgeYears)
        {
            items.Add(new AdultImmunizationScheduleItem(
                "Pneumococcal",
                asOfDate,
                "Pneumococcal vaccination for adults 65+"));
        }

        items.Add(new AdultImmunizationScheduleItem(
            "COVID-19",
            asOfDate,
            "COVID-19 booster"));

        return items
            .GroupBy(item => item.VaccineName, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(item => item.RecommendedDate)
            .ToList();
    }

    private static int CalculateAgeYears(DateOnly dateOfBirth, DateOnly asOfDate)
    {
        var age = asOfDate.Year - dateOfBirth.Year;
        if (dateOfBirth.AddYears(age) > asOfDate)
        {
            age--;
        }

        return age;
    }
}
