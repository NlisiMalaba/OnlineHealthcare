using HealthPlatform.Domain.Pharmacy;
using HealthPlatform.Domain.Prescriptions;

namespace HealthPlatform.Application.PharmacyOrders;

public interface IMedicationOrderRepository
{
    Task AddWithDispensedPrescriptionAsync(
        MedicationOrder order,
        Prescription prescription,
        CancellationToken ct);
}
