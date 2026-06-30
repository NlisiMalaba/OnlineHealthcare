using HealthPlatform.Domain.Payments;

namespace HealthPlatform.Application.Payments;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid paymentId, CancellationToken ct);

    Task<Payment?> GetByIdForPatientAsync(Guid paymentId, Guid patientId, CancellationToken ct);

    Task<IReadOnlyList<Payment>> ListForPatientAsync(Guid patientId, CancellationToken ct);

    Task AddAsync(Payment payment, CancellationToken ct);

    Task UpdateAsync(Payment payment, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
