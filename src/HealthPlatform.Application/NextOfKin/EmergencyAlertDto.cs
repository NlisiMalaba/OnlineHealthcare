using HealthPlatform.Domain.NextOfKin;

namespace HealthPlatform.Application.NextOfKin;

public sealed record EmergencyAlertContactDeliveryDto(
    Guid NextOfKinContactId,
    EmergencyAlertChannelDeliveryStatus SmsStatus,
    EmergencyAlertChannelDeliveryStatus PushStatus);

public sealed record EmergencyAlertDto(
    Guid Id,
    Guid PatientId,
    EmergencyAlertTriggerSource TriggerSource,
    Guid? TriggeredByDoctorId,
    Guid? AppointmentId,
    string TriggerReason,
    DateTime TriggeredAtUtc,
    EmergencyAlertOverallStatus OverallStatus,
    IReadOnlyList<EmergencyAlertContactDeliveryDto> ContactDeliveries);
