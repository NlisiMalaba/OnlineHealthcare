using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class HealthRecordAccessRepository(ApplicationDbContext db) : IHealthRecordAccessRepository
{
    public Task<HealthRecordAccess?> GetActiveGrantAsync(
        Guid healthRecordId,
        Guid doctorId,
        CancellationToken ct) =>
        db.HealthRecordAccesses
            .AsNoTracking()
            .FirstOrDefaultAsync(
                access => access.HealthRecordId == healthRecordId
                    && access.GrantedToDoctorId == doctorId
                    && access.RevokedAtUtc == null,
                ct);

    public Task<HealthRecordAccess?> GetLatestGrantAsync(
        Guid healthRecordId,
        Guid doctorId,
        CancellationToken ct) =>
        db.HealthRecordAccesses
            .FirstOrDefaultAsync(
                access => access.HealthRecordId == healthRecordId
                    && access.GrantedToDoctorId == doctorId,
                ct);

    public async Task<IReadOnlyList<HealthRecordAccess>> ListByHealthRecordIdAsync(
        Guid healthRecordId,
        CancellationToken ct) =>
        await db.HealthRecordAccesses
            .AsNoTracking()
            .Where(access => access.HealthRecordId == healthRecordId)
            .OrderByDescending(access => access.GrantedAtUtc)
            .ToListAsync(ct);

    public async Task AddAsync(HealthRecordAccess access, CancellationToken ct)
    {
        await db.HealthRecordAccesses.AddAsync(access, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(HealthRecordAccess access, CancellationToken ct)
    {
        db.HealthRecordAccesses.Update(access);
        await db.SaveChangesAsync(ct);
    }
}
