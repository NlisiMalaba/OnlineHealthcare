using HealthPlatform.Application.Prescriptions;
using HealthPlatform.Domain.Prescriptions;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class PrescriptionRepository(ApplicationDbContext db) : IPrescriptionRepository
{
    public async Task AddAsync(Prescription prescription, CancellationToken ct)
    {
        await db.Prescriptions.AddAsync(prescription, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task<Prescription?> GetByIdAsync(Guid prescriptionId, CancellationToken ct) =>
        db.Prescriptions.SingleOrDefaultAsync(p => p.Id == prescriptionId, ct);
}
