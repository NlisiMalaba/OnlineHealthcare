using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class LicenseVerificationQueueRepository(ApplicationDbContext db)
    : ILicenseVerificationQueueRepository
{
    public async Task EnqueueAsync(LicenseVerificationQueueItem item, CancellationToken ct)
    {
        await db.LicenseVerificationQueue.AddAsync(item, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task<bool> ExistsPendingForDoctorAsync(Guid doctorId, CancellationToken ct) =>
        db.LicenseVerificationQueue.AnyAsync(
            q => q.DoctorId == doctorId && !q.IsCompleted,
            ct);

    public Task<LicenseVerificationQueueItem?> GetPendingByDoctorIdAsync(Guid doctorId, CancellationToken ct) =>
        db.LicenseVerificationQueue.SingleOrDefaultAsync(
            q => q.DoctorId == doctorId && !q.IsCompleted,
            ct);

    public async Task UpdateAsync(LicenseVerificationQueueItem item, CancellationToken ct)
    {
        db.LicenseVerificationQueue.Update(item);
        await db.SaveChangesAsync(ct);
    }
}
