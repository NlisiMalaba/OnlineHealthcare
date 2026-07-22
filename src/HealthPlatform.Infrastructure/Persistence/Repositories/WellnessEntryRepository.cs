using HealthPlatform.Application.Wellness.WellnessEntries;
using HealthPlatform.Domain.Wellness;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class WellnessEntryRepository(ApplicationDbContext db) : IWellnessEntryRepository
{
    public async Task AddAsync(WellnessEntry entry, CancellationToken ct) =>
        await db.WellnessEntries.AddAsync(entry, ct);

    public async Task<IReadOnlyList<WellnessEntry>> ListByPatientIdAsync(
        Guid patientId,
        WellnessMetricType? metricType,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken ct)
    {
        var query = db.WellnessEntries
            .AsNoTracking()
            .Where(entry => entry.PatientId == patientId);

        if (metricType.HasValue)
        {
            query = query.Where(entry => entry.MetricType == metricType.Value);
        }

        if (fromUtc.HasValue)
        {
            query = query.Where(entry => entry.RecordedAtUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(entry => entry.RecordedAtUtc <= toUtc.Value);
        }

        return await query
            .OrderByDescending(entry => entry.RecordedAtUtc)
            .ToListAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
