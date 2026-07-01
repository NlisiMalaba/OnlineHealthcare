using HealthPlatform.Domain.NextOfKin;

namespace HealthPlatform.Application.NextOfKin;

public interface IEmergencyAlertRepository
{
    Task AddAsync(EmergencyAlert alert, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
