using HealthPlatform.Domain.NextOfKin;

namespace HealthPlatform.Application.NextOfKin;

public interface INextOfKinEmergencyAlertNotifier
{
    Task<IReadOnlyList<EmergencyAlertContactDeliveryResult>> NotifyAllContactsAsync(
        Guid emergencyAlertId,
        Guid patientId,
        string patientFullName,
        string triggerReason,
        IReadOnlyList<NextOfKinContactDto> contacts,
        CancellationToken ct);
}
