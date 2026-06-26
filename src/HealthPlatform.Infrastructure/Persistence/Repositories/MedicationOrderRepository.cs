using HealthPlatform.Application.PharmacyOrders;
using HealthPlatform.Domain.Pharmacy;
using HealthPlatform.Domain.Prescriptions;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class MedicationOrderRepository(ApplicationDbContext db) : IMedicationOrderRepository
{
    public async Task AddWithDispensedPrescriptionAsync(
        MedicationOrder order,
        Prescription prescription,
        CancellationToken ct)
    {
        await db.MedicationOrders.AddAsync(order, ct);
        db.Prescriptions.Update(prescription);
        await db.SaveChangesAsync(ct);
    }

    public Task<MedicationOrder?> GetByIdForPharmacyAsync(Guid orderId, Guid pharmacyId, CancellationToken ct) =>
        db.MedicationOrders.SingleOrDefaultAsync(
            order => order.Id == orderId && order.PharmacyId == pharmacyId,
            ct);

    public Task UpdateAsync(MedicationOrder order, CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
