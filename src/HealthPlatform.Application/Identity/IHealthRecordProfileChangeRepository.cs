using HealthPlatform.Domain.HealthRecords;

namespace HealthPlatform.Application.Identity;

public interface IHealthRecordProfileChangeRepository
{
    Task AddRangeAsync(IReadOnlyList<HealthRecordProfileChange> changes, CancellationToken ct);
}
