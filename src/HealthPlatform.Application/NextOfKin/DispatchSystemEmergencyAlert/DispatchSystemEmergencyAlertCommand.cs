using HealthPlatform.Application.Behaviors;
using HealthPlatform.Domain.NextOfKin;

namespace HealthPlatform.Application.NextOfKin.DispatchSystemEmergencyAlert;

public sealed record DispatchSystemEmergencyAlertCommand(
    Guid PatientId,
    string TriggerReason) : ICommand<EmergencyAlertDto>;
