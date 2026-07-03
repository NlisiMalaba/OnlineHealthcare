using HealthPlatform.Domain.NextOfKin;

namespace HealthPlatform.Application.NextOfKin;

public sealed record EmergencyAlertDispatchRequest(
    Guid PatientId,
    EmergencyAlertTriggerSource TriggerSource,
    string TriggerReason,
    Guid? TriggeredByDoctorId = null,
    Guid? AppointmentId = null);
