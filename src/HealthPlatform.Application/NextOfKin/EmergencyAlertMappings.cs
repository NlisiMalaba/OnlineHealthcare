using HealthPlatform.Domain.NextOfKin;

namespace HealthPlatform.Application.NextOfKin;

public static class EmergencyAlertMappings
{
    public static EmergencyAlertDto ToDto(this EmergencyAlert alert) =>
        new(
            alert.Id,
            alert.PatientId,
            alert.TriggerSource,
            alert.TriggeredByDoctorId,
            alert.AppointmentId,
            alert.TriggerReason,
            alert.TriggeredAtUtc,
            alert.OverallStatus,
            alert.ContactDeliveries
                .Select(delivery => delivery.ToDto())
                .ToList());

    public static EmergencyAlertContactDeliveryDto ToDto(this EmergencyAlertContactDelivery delivery) =>
        new(
            delivery.NextOfKinContactId,
            delivery.SmsStatus,
            delivery.PushStatus);
}
