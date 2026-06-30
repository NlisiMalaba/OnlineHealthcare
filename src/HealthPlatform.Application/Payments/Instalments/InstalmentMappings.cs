using HealthPlatform.Domain.Payments.Instalments;

namespace HealthPlatform.Application.Payments.Instalments;

public static class InstalmentMappings
{
    public static InstalmentScheduleItemDto ToDto(this InstalmentScheduleEntry entry) =>
        new(entry.SequenceNumber, entry.AmountMinorUnits, entry.DueDate);

    public static InstalmentScheduleItemDto ToDto(this InstalmentPayment payment) =>
        new(payment.SequenceNumber, payment.AmountMinorUnits + payment.LateFeeMinorUnits, payment.DueDate);

    public static InstalmentPlanPreviewDto ToPreviewDto(
        long totalAmountMinorUnits,
        InstalmentFrequency frequency,
        int instalmentCount,
        string currency,
        DateOnly firstDueDate,
        IReadOnlyList<InstalmentScheduleEntry> schedule) =>
        new(
            totalAmountMinorUnits,
            schedule[0].AmountMinorUnits,
            totalAmountMinorUnits,
            frequency,
            instalmentCount,
            currency.Trim().ToUpperInvariant(),
            firstDueDate,
            schedule.Select(entry => entry.ToDto()).ToList());

    public static InstalmentPlanDto ToDto(
        this InstalmentPlan plan,
        IReadOnlyList<InstalmentPayment> payments) =>
        new(
            plan.Id,
            plan.TotalAmountMinorUnits,
            plan.InstalmentAmountMinorUnits,
            plan.TotalRepayableMinorUnits,
            plan.Frequency,
            plan.TotalInstalments,
            plan.PaidInstalments,
            plan.Status,
            plan.Currency,
            plan.FirstDueDate,
            payments.OrderBy(p => p.SequenceNumber).Select(p => p.ToDto()).ToList());

    public static InstalmentPlanListItemDto ToListItemDto(this InstalmentPlan plan) =>
        new(
            plan.Id,
            plan.TotalAmountMinorUnits,
            plan.TotalRepayableMinorUnits,
            plan.Frequency,
            plan.TotalInstalments,
            plan.PaidInstalments,
            plan.Status,
            plan.Currency,
            plan.FirstDueDate);
}
