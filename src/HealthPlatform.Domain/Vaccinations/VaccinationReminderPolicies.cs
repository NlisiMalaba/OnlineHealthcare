namespace HealthPlatform.Domain.Vaccinations;

public static class VaccinationReminderPolicies
{
    public const int ReminderLeadDays = 7;

    public static bool IsDueForReminder(DateOnly recommendedDate, DateOnly asOfDate) =>
        recommendedDate >= asOfDate
        && recommendedDate <= asOfDate.AddDays(ReminderLeadDays);
}
