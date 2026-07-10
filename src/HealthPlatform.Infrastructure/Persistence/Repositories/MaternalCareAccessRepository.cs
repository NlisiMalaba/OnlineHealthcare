using HealthPlatform.Application.Maternal.BirthPlans;
using HealthPlatform.Domain.Maternal;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class MaternalCareAccessRepository(ApplicationDbContext db) : IMaternalCareAccessRepository
{
    public async Task AddAsync(MaternalCareAccessGrant grant, CancellationToken ct)
    {
        await db.MaternalCareAccessGrants.AddAsync(grant, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task UpdateAsync(MaternalCareAccessGrant grant, CancellationToken ct) =>
        db.SaveChangesAsync(ct);

    public Task<MaternalCareAccessGrant?> GetActiveGrantAsync(
        Guid antenatalRecordId,
        Guid doctorId,
        CancellationToken ct) =>
        db.MaternalCareAccessGrants.SingleOrDefaultAsync(
            grant => grant.AntenatalRecordId == antenatalRecordId
                && grant.DoctorId == doctorId
                && grant.RevokedAtUtc == null,
            ct);

    public Task<MaternalCareAccessGrant?> GetLatestGrantAsync(
        Guid antenatalRecordId,
        Guid doctorId,
        CancellationToken ct) =>
        db.MaternalCareAccessGrants
            .Where(grant => grant.AntenatalRecordId == antenatalRecordId && grant.DoctorId == doctorId)
            .OrderByDescending(grant => grant.GrantedAtUtc)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<MaternalCareAccessGrant>> ListActiveGrantsByAntenatalRecordIdAsync(
        Guid antenatalRecordId,
        CancellationToken ct) =>
        await db.MaternalCareAccessGrants
            .Where(grant => grant.AntenatalRecordId == antenatalRecordId && grant.RevokedAtUtc == null)
            .OrderByDescending(grant => grant.GrantedAtUtc)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<MaternalCareAccessGrant>> ListActiveGrantsByPatientIdAsync(
        Guid patientId,
        CancellationToken ct) =>
        await db.MaternalCareAccessGrants
            .Where(grant => grant.PatientId == patientId && grant.RevokedAtUtc == null)
            .OrderByDescending(grant => grant.GrantedAtUtc)
            .ToListAsync(ct);
}
