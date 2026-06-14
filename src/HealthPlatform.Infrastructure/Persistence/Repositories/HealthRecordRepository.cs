using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class HealthRecordRepository(ApplicationDbContext db) : IHealthRecordRepository
{
    public Task<bool> ExistsForPatientAsync(Guid patientId, CancellationToken ct) =>
        db.HealthRecords.AnyAsync(r => r.PatientId == patientId, ct);

    public Task<HealthRecord?> GetByPatientIdAsync(Guid patientId, CancellationToken ct) =>
        db.HealthRecords.FirstOrDefaultAsync(r => r.PatientId == patientId, ct);

    public Task AddAsync(HealthRecord healthRecord, CancellationToken ct) =>
        db.HealthRecords.AddAsync(healthRecord, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
