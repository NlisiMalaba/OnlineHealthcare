using MediatR;

namespace HealthPlatform.Application.NextOfKin.DispatchSystemEmergencyAlert;

public sealed class DispatchSystemEmergencyAlertCommandHandler(
    IEmergencyAlertDispatchService emergencyAlertDispatchService)
    : IRequestHandler<DispatchSystemEmergencyAlertCommand, EmergencyAlertDto>
{
    public Task<EmergencyAlertDto> Handle(DispatchSystemEmergencyAlertCommand request, CancellationToken ct) =>
        emergencyAlertDispatchService.DispatchAsync(
            new EmergencyAlertDispatchRequest(
                request.PatientId,
                Domain.NextOfKin.EmergencyAlertTriggerSource.System,
                request.TriggerReason),
            ct);
}
