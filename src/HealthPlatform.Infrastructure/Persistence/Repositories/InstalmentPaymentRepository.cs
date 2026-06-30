using HealthPlatform.Application.Payments.Instalments;
using HealthPlatform.Domain.Payments.Instalments;
using HealthPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class InstalmentPaymentRepository(ApplicationDbContext db) : IInstalmentPaymentRepository
{
    public async Task<IReadOnlyList<InstalmentPayment>> ListForPlanAsync(Guid instalmentPlanId, CancellationToken ct) =>
        await db.InstalmentPayments
            .Where(p => p.InstalmentPlanId == instalmentPlanId)
            .OrderBy(p => p.SequenceNumber)
            .ToListAsync(ct);

    public async Task AddRangeAsync(IReadOnlyList<InstalmentPayment> payments, CancellationToken ct)
    {
        await db.InstalmentPayments.AddRangeAsync(payments, ct);
    }

    public async Task<IReadOnlyList<InstalmentPayment>> ListDueRemindersAsync(DateTime nowUtc, int take, CancellationToken ct)
    {
        var reminderWindowEnd = DateOnly.FromDateTime(nowUtc.AddHours(InstalmentPolicies.ReminderLeadHours));

        return await db.InstalmentPayments
            .Where(p => p.Status == InstalmentPaymentStatus.Scheduled
                        && !p.DueReminderSent
                        && p.DueDate <= reminderWindowEnd
                        && p.DueDate >= DateOnly.FromDateTime(nowUtc))
            .OrderBy(p => p.DueDate)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<InstalmentPayment>> ListMissedCandidatesAsync(DateTime nowUtc, int take, CancellationToken ct)
    {
        var missedCutoff = DateOnly.FromDateTime(
            nowUtc.AddHours(-InstalmentPolicies.MissedPaymentGraceHours));

        return await db.InstalmentPayments
            .Where(p => p.Status == InstalmentPaymentStatus.Scheduled
                        && p.DueDate <= missedCutoff)
            .OrderBy(p => p.DueDate)
            .Take(take)
            .ToListAsync(ct);
    }

    public Task UpdateAsync(InstalmentPayment payment, CancellationToken ct)
    {
        db.InstalmentPayments.Update(payment);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
