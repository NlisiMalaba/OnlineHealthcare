using HealthPlatform.Application.PharmacyOrders;
using HealthPlatform.Domain.Pharmacy;
using HealthPlatform.Domain.Prescriptions;
using HealthPlatform.Infrastructure.Persistence;
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
}
