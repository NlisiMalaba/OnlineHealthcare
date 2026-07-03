using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Domain.NextOfKin;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingNextOfKinEmergencyAlertNotifier : INextOfKinEmergencyAlertNotifier
{
    public List<EmergencyAlertDispatchCall> Calls { get; } = [];

    public Task<IReadOnlyList<EmergencyAlertContactDeliveryResult>> NotifyAllContactsAsync(
        Guid emergencyAlertId,
        Guid patientId,
        string patientFullName,
        string triggerReason,
        IReadOnlyList<NextOfKinContactDto> contacts,
        CancellationToken ct)
    {
        Calls.Add(new EmergencyAlertDispatchCall(
            emergencyAlertId,
            patientId,
            triggerReason,
            contacts.Select(contact => contact.Id).ToList()));

        IReadOnlyList<EmergencyAlertContactDeliveryResult> results = contacts
            .Select(contact => new EmergencyAlertContactDeliveryResult(
                contact.Id,
                EmergencyAlertChannelDeliveryStatus.Sent,
                EmergencyAlertChannelDeliveryStatus.Sent))
            .ToList();

        return Task.FromResult(results);
    }

    public sealed record EmergencyAlertDispatchCall(
        Guid EmergencyAlertId,
        Guid PatientId,
        string TriggerReason,
        IReadOnlyList<Guid> ContactIds);
}
