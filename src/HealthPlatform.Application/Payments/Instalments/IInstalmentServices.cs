using HealthPlatform.Domain.Payments.Instalments;

namespace HealthPlatform.Application.Payments.Instalments;

public interface IInstalmentPlanRepository
{
    Task<InstalmentPlan?> GetByIdAsync(Guid planId, CancellationToken ct);

    Task<InstalmentPlan?> GetByIdForPatientAsync(Guid planId, Guid patientId, CancellationToken ct);

    Task<IReadOnlyList<InstalmentPlan>> ListForPatientAsync(Guid patientId, CancellationToken ct);

    Task AddAsync(InstalmentPlan plan, CancellationToken ct);

    Task UpdateAsync(InstalmentPlan plan, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}

public interface IInstalmentPaymentRepository
{
    Task<IReadOnlyList<InstalmentPayment>> ListForPlanAsync(Guid instalmentPlanId, CancellationToken ct);

    Task AddRangeAsync(IReadOnlyList<InstalmentPayment> payments, CancellationToken ct);

    Task<IReadOnlyList<InstalmentPayment>> ListDueRemindersAsync(DateTime nowUtc, int take, CancellationToken ct);

    Task<IReadOnlyList<InstalmentPayment>> ListMissedCandidatesAsync(DateTime nowUtc, int take, CancellationToken ct);

    Task UpdateAsync(InstalmentPayment payment, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}

public interface IInstalmentDueReminderNotifier
{
    Task NotifyDueReminderAsync(
        Guid patientUserId,
        Guid patientId,
        Guid instalmentPlanId,
        Guid instalmentPaymentId,
        int sequenceNumber,
        long amountMinorUnits,
        string currency,
        DateOnly dueDate,
        CancellationToken ct);
}

public interface IInstalmentMissedPaymentNotifier
{
    Task NotifyMissedPaymentAsync(
        Guid patientUserId,
        Guid patientId,
        Guid instalmentPlanId,
        Guid instalmentPaymentId,
        int sequenceNumber,
        long amountMinorUnits,
        long lateFeeMinorUnits,
        string currency,
        DateOnly dueDate,
        CancellationToken ct);
}

public interface IInstalmentDueReminderDispatcher
{
    Task<int> DispatchDueRemindersAsync(CancellationToken ct);
}

public interface IInstalmentMissedPaymentProcessor
{
    Task<int> ProcessMissedPaymentsAsync(CancellationToken ct);
}

public sealed class InstalmentPlanOptions
{
    public const string SectionName = "Payments:Instalments";

    public long MinimumExpenseMinorUnits { get; set; } = 10_000;

    public long LateFeeMinorUnits { get; set; } = 500;

    public int MaxInstalments { get; set; } = 12;
}
