using HealthPlatform.Application.NextOfKin;

namespace HealthPlatform.Application.Wellness;

public interface IConsecutiveMissedDosesNextOfKinNotifier
{
    Task NotifyConsecutiveMissedDosesAsync(
        Guid patientId,
        Guid triggeringAdherenceEventId,
        IReadOnlyList<NextOfKinContactDto> contacts,
        CancellationToken ct);
}
