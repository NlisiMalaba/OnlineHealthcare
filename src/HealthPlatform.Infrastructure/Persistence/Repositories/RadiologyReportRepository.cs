using HealthPlatform.Application.Labs;
using HealthPlatform.Domain.Labs;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class RadiologyReportRepository(ApplicationDbContext db) : IRadiologyReportRepository
{
    public Task<RadiologyReport?> GetByIdAsync(Guid radiologyReportId, CancellationToken ct) =>
        db.RadiologyReports.FirstOrDefaultAsync(x => x.Id == radiologyReportId, ct);

    public Task AddAsync(RadiologyReport report, CancellationToken ct) =>
        db.RadiologyReports.AddAsync(report, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
