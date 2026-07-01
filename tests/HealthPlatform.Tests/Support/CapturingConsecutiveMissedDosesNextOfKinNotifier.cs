using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Application.Wellness;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingConsecutiveMissedDosesNextOfKinNotifier : IConsecutiveMissedDosesNextOfKinNotifier
{
    public List<ConsecutiveMissedDosesAlertCall> Calls { get; } = [];

    public Task NotifyConsecutiveMissedDosesAsync(
        Guid patientId,
        Guid triggeringAdherenceEventId,
        IReadOnlyList<NextOfKinContactDto> contacts,
        CancellationToken ct)
    {
        Calls.Add(new ConsecutiveMissedDosesAlertCall(
            patientId,
            triggeringAdherenceEventId,
            contacts.Select(contact => contact.Id).ToList()));
        return Task.CompletedTask;
    }

    public sealed record ConsecutiveMissedDosesAlertCall(
        Guid PatientId,
        Guid TriggeringAdherenceEventId,
        IReadOnlyList<Guid> ContactIds);
}
