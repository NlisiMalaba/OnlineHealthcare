using HealthPlatform.Application.Vaccinations;
using HealthPlatform.Domain.Vaccinations;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class VaccinationRecordRepository(ApplicationDbContext db) : IVaccinationRecordRepository
{
    public Task AddAsync(VaccinationRecord record, CancellationToken ct) =>
        db.VaccinationRecords.AddAsync(record, ct).AsTask();

    public async Task<IReadOnlyList<VaccinationRecord>> ListByChildProfileIdAsync(
        Guid childProfileId,
        CancellationToken ct) =>
        await db.VaccinationRecords
            .Where(record => record.ChildProfileId == childProfileId)
            .OrderByDescending(record => record.AdministeredDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<VaccinationRecord>> ListByPatientIdAsync(
        Guid patientId,
        CancellationToken ct) =>
        await db.VaccinationRecords
            .Where(record => record.PatientId == patientId)
            .OrderByDescending(record => record.AdministeredDate)
            .ToListAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
