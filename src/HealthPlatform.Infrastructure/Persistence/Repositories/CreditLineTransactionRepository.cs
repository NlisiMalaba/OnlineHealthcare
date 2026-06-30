using HealthPlatform.Application.Payments.CreditLine;
using HealthPlatform.Domain.Payments.CreditLine;
using HealthPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class CreditLineTransactionRepository(ApplicationDbContext db) : ICreditLineTransactionRepository
{
    public async Task AddAsync(CreditLineTransaction transaction, CancellationToken ct)
    {
        await db.CreditLineTransactions.AddAsync(transaction, ct);
    }

    public async Task<IReadOnlyList<CreditLineTransaction>> ListForPatientAsync(
        Guid patientId,
        CancellationToken ct) =>
        await db.CreditLineTransactions
            .Where(t => t.PatientId == patientId)
            .OrderByDescending(t => t.CreatedAtUtc)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<CreditLineTransaction>> ListDueRepaymentRemindersAsync(
        DateTime dueBeforeUtc,
        int take,
        CancellationToken ct) =>
        await db.CreditLineTransactions
            .Where(t => !t.RepaymentReminderSent
                        && t.TransactionType == CreditLineTransactionType.Charge
                        && t.RepaymentDueAtUtc <= dueBeforeUtc)
            .OrderBy(t => t.RepaymentDueAtUtc)
            .Take(take)
            .ToListAsync(ct);

    public Task UpdateAsync(CreditLineTransaction transaction, CancellationToken ct)
    {
        db.CreditLineTransactions.Update(transaction);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
