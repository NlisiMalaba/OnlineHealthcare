using HealthPlatform.Domain.NextOfKin;

namespace HealthPlatform.Application.NextOfKin;

public interface IEmergencyAlertRepository
{
    Task AddAsync(EmergencyAlert alert, CancellationToken ct);

    Task<EmergencyAlert?> GetByIdAsync(Guid alertId, CancellationToken ct);

    Task<EmergencyAlertContactDelivery?> GetContactDeliveryAsync(
        Guid alertId,
        Guid contactId,
        CancellationToken ct);

    Task<IReadOnlyList<EmergencyAlertContactDelivery>> ListContactDeliveriesAsync(
        Guid alertId,
        CancellationToken ct);

    Task UpdateContactDeliveryAsync(EmergencyAlertContactDelivery delivery, CancellationToken ct);

    Task UpdateOverallStatusAsync(Guid alertId, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
