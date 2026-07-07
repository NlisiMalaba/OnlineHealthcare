using HealthPlatform.Application.Labs;
using HealthPlatform.Domain.Labs;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class RadiologyReportRepository(ApplicationDbContext db) : IRadiologyReportRepository
{
    public Task AddAsync(RadiologyReport report, CancellationToken ct) =>
        db.RadiologyReports.AddAsync(report, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
