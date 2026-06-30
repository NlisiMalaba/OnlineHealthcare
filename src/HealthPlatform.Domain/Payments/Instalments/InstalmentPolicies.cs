namespace HealthPlatform.Domain.Payments.Instalments;

public sealed record InstalmentScheduleEntry(
    int SequenceNumber,
    long AmountMinorUnits,
    DateOnly DueDate);

public static class InstalmentPolicies
{
    public const int ReminderLeadHours = 24;

    public const int MissedPaymentGraceHours = 24;

    public static bool MeetsMinimumExpense(long totalMinorUnits, long minimumExpenseMinorUnits) =>
        totalMinorUnits >= minimumExpenseMinorUnits;

    public static IReadOnlyList<InstalmentScheduleEntry> BuildSchedule(
        long totalMinorUnits,
        int instalmentCount,
        InstalmentFrequency frequency,
        DateOnly firstDueDate)
    {
        if (totalMinorUnits <= 0)
        {
            throw new InvalidInstalmentPlanException("Total amount must be positive.");
        }

        if (instalmentCount <= 0)
        {
            throw new InvalidInstalmentPlanException("Instalment count must be positive.");
        }

        var baseAmount = totalMinorUnits / instalmentCount;
        var remainder = totalMinorUnits % instalmentCount;
        var schedule = new List<InstalmentScheduleEntry>(instalmentCount);
        var dueDate = firstDueDate;

        for (var sequence = 1; sequence <= instalmentCount; sequence++)
        {
            var amount = baseAmount + (sequence == instalmentCount ? remainder : 0);
            schedule.Add(new InstalmentScheduleEntry(sequence, amount, dueDate));
            dueDate = AdvanceDueDate(dueDate, frequency);
        }

        return schedule;
    }

    public static DateOnly AdvanceDueDate(DateOnly dueDate, InstalmentFrequency frequency) =>
        frequency switch
        {
            InstalmentFrequency.Weekly => dueDate.AddDays(7),
            InstalmentFrequency.Biweekly => dueDate.AddDays(14),
            InstalmentFrequency.Monthly => dueDate.AddMonths(1),
            _ => throw new ArgumentOutOfRangeException(nameof(frequency), frequency, "Unsupported instalment frequency.")
        };

    public static bool IsDueForReminder(DateOnly dueDate, DateTime nowUtc) =>
        dueDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) - nowUtc <= TimeSpan.FromHours(ReminderLeadHours)
        && dueDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) > nowUtc;

    public static bool IsMissed(DateOnly dueDate, DateTime nowUtc) =>
        nowUtc >= dueDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddHours(MissedPaymentGraceHours);
}
