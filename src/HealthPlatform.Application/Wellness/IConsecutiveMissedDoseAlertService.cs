using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness;

public interface IConsecutiveMissedDoseAlertService
{
    Task TryEmitAlertIfThresholdReachedAsync(
        Guid patientId,
        CancellationToken ct);
}
