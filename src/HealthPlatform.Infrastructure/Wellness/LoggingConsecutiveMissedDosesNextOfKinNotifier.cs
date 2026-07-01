using HealthPlatform.Application.Wellness;
using HealthPlatform.Application.NextOfKin;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Wellness;

public sealed class LoggingConsecutiveMissedDosesNextOfKinNotifier(
    ILogger<LoggingConsecutiveMissedDosesNextOfKinNotifier> logger) : IConsecutiveMissedDosesNextOfKinNotifier
{
    public Task NotifyConsecutiveMissedDosesAsync(
        Guid patientId,
        Guid triggeringAdherenceEventId,
        IReadOnlyList<NextOfKinContactDto> contacts,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Consecutive missed dose alert requested for patient {PatientId}, adherence event {AdherenceEventId}, notifying {ContactCount} next-of-kin contact(s).",
            patientId,
            triggeringAdherenceEventId,
            contacts.Count);
        return Task.CompletedTask;
    }
}
