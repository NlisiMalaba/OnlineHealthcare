using HealthPlatform.Domain.Payments.CreditLine;

namespace HealthPlatform.Application.Payments.CreditLine;

public interface IPatientCreditLineRepository
{
    Task<PatientCreditLine?> GetByPatientIdAsync(Guid patientId, CancellationToken ct);

    Task AddAsync(PatientCreditLine creditLine, CancellationToken ct);

    Task UpdateAsync(PatientCreditLine creditLine, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}

public interface ICreditLineTransactionRepository
{
    Task AddAsync(CreditLineTransaction transaction, CancellationToken ct);

    Task<IReadOnlyList<CreditLineTransaction>> ListForPatientAsync(Guid patientId, CancellationToken ct);

    Task<IReadOnlyList<CreditLineTransaction>> ListDueRepaymentRemindersAsync(
        DateTime dueBeforeUtc,
        int take,
        CancellationToken ct);

    Task UpdateAsync(CreditLineTransaction transaction, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
