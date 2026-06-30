using HealthPlatform.Domain.Pharmacy;
using HealthPlatform.Domain.Prescriptions;

namespace HealthPlatform.Application.PharmacyOrders;

public interface IMedicationOrderRepository
{
    Task AddWithDispensedPrescriptionAsync(
        MedicationOrder order,
        Prescription prescription,
        CancellationToken ct);

    Task<MedicationOrder?> GetByIdAsync(Guid orderId, CancellationToken ct);

    Task<MedicationOrder?> GetByIdForPharmacyAsync(Guid orderId, Guid pharmacyId, CancellationToken ct);

    Task UpdateAsync(MedicationOrder order, CancellationToken ct);
}
