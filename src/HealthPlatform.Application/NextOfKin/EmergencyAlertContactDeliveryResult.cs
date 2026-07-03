using HealthPlatform.Domain.NextOfKin;

namespace HealthPlatform.Application.NextOfKin;

public sealed record EmergencyAlertContactDeliveryResult(
    Guid NextOfKinContactId,
    EmergencyAlertChannelDeliveryStatus SmsStatus,
    EmergencyAlertChannelDeliveryStatus PushStatus);
