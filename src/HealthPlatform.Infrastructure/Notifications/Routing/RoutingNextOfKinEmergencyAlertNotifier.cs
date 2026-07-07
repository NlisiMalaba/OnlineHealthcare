using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Application.Notifications;
using HealthPlatform.Domain.NextOfKin;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingNextOfKinEmergencyAlertNotifier(INotificationDispatcher dispatcher)
    : INextOfKinEmergencyAlertNotifier
{
    public async Task<IReadOnlyList<EmergencyAlertContactDeliveryResult>> NotifyAllContactsAsync(
        Guid emergencyAlertId,
        Guid patientId,
        string patientFullName,
        string triggerReason,
        IReadOnlyList<NextOfKinContactDto> contacts,
        CancellationToken ct)
    {
        var results = new List<EmergencyAlertContactDeliveryResult>(contacts.Count);
        foreach (var contact in contacts)
        {
            var smsResult = await NotificationRoutingSupport.DispatchToContactAsync(
                dispatcher,
                NotificationRecipientType.NextOfKin,
                NotificationEventTypes.EmergencyAlert,
                NotificationCriticality.Critical,
                "Emergency alert",
                "An emergency alert has been triggered for a patient you are listed as next-of-kin for.",
                new NotificationContactOverride(contact.Email, contact.PhoneNumber),
                new Dictionary<string, string>
                {
                    ["emergency_alert_id"] = emergencyAlertId.ToString(),
                    ["patient_id"] = patientId.ToString(),
                    ["contact_id"] = contact.Id.ToString()
                },
                ct,
                [NotificationChannel.Sms]);

            var pushResult = await NotificationRoutingSupport.DispatchToContactAsync(
                dispatcher,
                NotificationRecipientType.NextOfKin,
                NotificationEventTypes.EmergencyAlert,
                NotificationCriticality.Critical,
                "Emergency alert",
                "An emergency alert has been triggered.",
                new NotificationContactOverride(contact.Email, contact.PhoneNumber, []),
                new Dictionary<string, string>
                {
                    ["emergency_alert_id"] = emergencyAlertId.ToString(),
                    ["patient_id"] = patientId.ToString(),
                    ["contact_id"] = contact.Id.ToString()
                },
                ct,
                [NotificationChannel.Push]);

            results.Add(new EmergencyAlertContactDeliveryResult(
                contact.Id,
                MapChannelStatus(smsResult, NotificationChannel.Sms),
                MapChannelStatus(pushResult, NotificationChannel.Push)));
        }

        return results;
    }

    private static EmergencyAlertChannelDeliveryStatus MapChannelStatus(
        NotificationDispatchResult result,
        NotificationChannel channel)
    {
        var channelResult = result.ChannelResults.FirstOrDefault(entry => entry.Channel == channel);
        return channelResult is { Succeeded: true }
            ? EmergencyAlertChannelDeliveryStatus.Sent
            : EmergencyAlertChannelDeliveryStatus.Failed;
    }
}
