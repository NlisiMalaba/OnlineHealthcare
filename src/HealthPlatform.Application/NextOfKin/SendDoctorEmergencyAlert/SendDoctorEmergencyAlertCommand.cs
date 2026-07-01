using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.NextOfKin.SendDoctorEmergencyAlert;

public sealed record SendDoctorEmergencyAlertCommand(
    Guid PatientId,
    Guid AppointmentId,
    string TriggerReason) : ICommand<EmergencyAlertDto>;
