namespace HealthPlatform.Application.NextOfKin;

public interface IEmergencyAlertDispatchService
{
    Task<EmergencyAlertDto> DispatchAsync(EmergencyAlertDispatchRequest request, CancellationToken ct);
}
