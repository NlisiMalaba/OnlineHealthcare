using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Domain.NextOfKin;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.NextOfKin;

public sealed class LoggingNextOfKinEmergencyAlertNotifier(
    ILogger<LoggingNextOfKinEmergencyAlertNotifier> logger) : INextOfKinEmergencyAlertNotifier
{
    public async Task<IReadOnlyList<EmergencyAlertContactDeliveryResult>> NotifyAllContactsAsync(
        Guid emergencyAlertId,
        Guid patientId,
        string patientFullName,
        string triggerReason,
        IReadOnlyList<NextOfKinContactDto> contacts,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var dispatchTasks = contacts.Select(contact => DispatchToContactAsync(
            emergencyAlertId,
            patientId,
            patientFullName,
            triggerReason,
            contact,
            ct));

        var results = await Task.WhenAll(dispatchTasks);
        return results;
    }

    private Task<EmergencyAlertContactDeliveryResult> DispatchToContactAsync(
        Guid emergencyAlertId,
        Guid patientId,
        string patientFullName,
        string triggerReason,
        NextOfKinContactDto contact,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Emergency alert {AlertId} dispatching SMS and push simultaneously to next-of-kin contact {ContactId} for patient {PatientId}. Reason: {TriggerReason}.",
            emergencyAlertId,
            contact.Id,
            patientId,
            triggerReason);

        return Task.FromResult(new EmergencyAlertContactDeliveryResult(
            contact.Id,
            EmergencyAlertChannelDeliveryStatus.Sent,
            EmergencyAlertChannelDeliveryStatus.Sent));
    }
}
