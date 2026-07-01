namespace HealthPlatform.Domain.NextOfKin;

public sealed class EmergencyAlertContactDelivery
{
    private EmergencyAlertContactDelivery()
    {
    }

    public Guid Id { get; private set; }

    public Guid EmergencyAlertId { get; private set; }

    public Guid NextOfKinContactId { get; private set; }

    public EmergencyAlertChannelDeliveryStatus SmsStatus { get; private set; }

    public EmergencyAlertChannelDeliveryStatus PushStatus { get; private set; }

    public static EmergencyAlertContactDelivery Create(
        Guid emergencyAlertId,
        Guid nextOfKinContactId,
        EmergencyAlertChannelDeliveryStatus smsStatus,
        EmergencyAlertChannelDeliveryStatus pushStatus)
    {
        if (emergencyAlertId == Guid.Empty)
        {
            throw new ArgumentException("Emergency alert id is required.", nameof(emergencyAlertId));
        }

        if (nextOfKinContactId == Guid.Empty)
        {
            throw new ArgumentException("Next-of-kin contact id is required.", nameof(nextOfKinContactId));
        }

        return new EmergencyAlertContactDelivery
        {
            Id = Guid.CreateVersion7(),
            EmergencyAlertId = emergencyAlertId,
            NextOfKinContactId = nextOfKinContactId,
            SmsStatus = smsStatus,
            PushStatus = pushStatus
        };
    }
}
