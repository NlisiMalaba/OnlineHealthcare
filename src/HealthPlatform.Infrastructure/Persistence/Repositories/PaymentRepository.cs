using HealthPlatform.Application.Payments;
using HealthPlatform.Domain.Payments;
using HealthPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class PaymentRepository(ApplicationDbContext db) : IPaymentRepository
{
    public Task<Payment?> GetByIdAsync(Guid paymentId, CancellationToken ct) =>
        db.Payments.SingleOrDefaultAsync(payment => payment.Id == paymentId, ct);

    public Task<Payment?> GetByIdForPatientAsync(Guid paymentId, Guid patientId, CancellationToken ct) =>
        db.Payments.SingleOrDefaultAsync(
            payment => payment.Id == paymentId && payment.PatientId == patientId,
            ct);

    public async Task<IReadOnlyList<Payment>> ListForPatientAsync(Guid patientId, CancellationToken ct) =>
        await db.Payments
            .Where(payment => payment.PatientId == patientId)
            .OrderByDescending(payment => payment.CompletedAtUtc ?? payment.FailedAtUtc ?? payment.CreatedAtUtc)
            .ToListAsync(ct);

    public async Task AddAsync(Payment payment, CancellationToken ct) =>
        await db.Payments.AddAsync(payment, ct);

    public Task UpdateAsync(Payment payment, CancellationToken ct)
    {
        db.Payments.Update(payment);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
