using HealthPlatform.Domain.NextOfKin;

namespace HealthPlatform.Application.NextOfKin;

public sealed record EmergencyAlertChannelDeliveryRequest(
    Guid EmergencyAlertId,
    Guid PatientId,
    string PatientFullName,
    string TriggerReason,
    NextOfKinContactDto Contact,
    NextOfKinNotificationChannel Channel);

public interface INextOfKinChannelDeliveryGateway
{
    Task<bool> TryDeliverEmergencyAlertAsync(
        EmergencyAlertChannelDeliveryRequest request,
        CancellationToken ct);
}
