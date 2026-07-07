using HealthPlatform.Domain.Labs;

namespace HealthPlatform.Application.Labs;

public interface IRadiologyReportRepository
{
    Task AddAsync(RadiologyReport report, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
