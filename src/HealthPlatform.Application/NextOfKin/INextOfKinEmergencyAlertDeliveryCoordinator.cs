using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.NextOfKin;

namespace HealthPlatform.Application.NextOfKin;

public interface INextOfKinEmergencyAlertDeliveryCoordinator
{
    Task<IReadOnlyList<EmergencyAlertContactDelivery>> DispatchAsync(
        EmergencyAlert alert,
        Patient patient,
        IReadOnlyList<NextOfKinContactDto> contacts,
        CancellationToken ct);
}
