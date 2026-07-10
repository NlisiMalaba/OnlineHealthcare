namespace HealthPlatform.Domain.Vaccinations;

public readonly record struct ChildImmunizationScheduleItem(
    int DaysFromBirth,
    string VaccineName,
    string Description);

public static class ChildImmunizationSchedulePolicies
{
    private static readonly ChildImmunizationScheduleItem[] NationalSchedule =
    [
        new(0, "BCG", "BCG at birth"),
        new(0, "Hepatitis B", "Hepatitis B birth dose"),
        new(0, "OPV", "Oral polio vaccine at birth"),
        new(42, "Pentavalent", "DPT-HepB-Hib dose 1"),
        new(42, "PCV", "Pneumococcal conjugate dose 1"),
        new(42, "Rotavirus", "Rotavirus dose 1"),
        new(70, "Pentavalent", "DPT-HepB-Hib dose 2"),
        new(70, "PCV", "Pneumococcal conjugate dose 2"),
        new(70, "Rotavirus", "Rotavirus dose 2"),
        new(98, "Pentavalent", "DPT-HepB-Hib dose 3"),
        new(98, "PCV", "Pneumococcal conjugate dose 3"),
        new(98, "OPV", "Oral polio vaccine dose 3"),
        new(270, "Measles", "Measles dose 1"),
        new(270, "Yellow Fever", "Yellow fever vaccine"),
        new(450, "Measles", "Measles dose 2"),
        new(450, "Pentavalent", "DPT-HepB-Hib booster")
    ];

    public static IReadOnlyList<ChildImmunizationScheduleItem> BuildRecommendedSchedule(
        DateOnly dateOfBirth,
        DateOnly asOfDate)
    {
        if (dateOfBirth > asOfDate)
        {
            throw new ArgumentException("Date of birth cannot be after the reference date.", nameof(dateOfBirth));
        }

        return NationalSchedule
            .Select(item => item with { })
            .Where(item => dateOfBirth.AddDays(item.DaysFromBirth) >= asOfDate)
            .ToList();
    }

    public static DateOnly ResolveRecommendedDate(DateOnly dateOfBirth, int daysFromBirth) =>
        dateOfBirth.AddDays(daysFromBirth);
}
