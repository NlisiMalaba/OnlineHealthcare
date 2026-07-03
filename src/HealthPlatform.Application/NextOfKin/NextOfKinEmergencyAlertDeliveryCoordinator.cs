using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.NextOfKin;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.NextOfKin;

public sealed class NextOfKinEmergencyAlertDeliveryCoordinator(
    TimeProvider timeProvider,
    INextOfKinEmergencyAlertNotifier emergencyAlertNotifier,
    INextOfKinNotificationDeliveryRepository notificationDeliveryRepository,
    ILogger<NextOfKinEmergencyAlertDeliveryCoordinator> logger) : INextOfKinEmergencyAlertDeliveryCoordinator
{
    public async Task<IReadOnlyList<EmergencyAlertContactDelivery>> DispatchAsync(
        EmergencyAlert alert,
        Patient patient,
        IReadOnlyList<NextOfKinContactDto> contacts,
        CancellationToken ct)
    {
        var deliveryResults = await emergencyAlertNotifier.NotifyAllContactsAsync(
            alert.Id,
            patient.Id,
            patient.FullName,
            alert.TriggerReason,
            contacts,
            ct);

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var retryDeliveries = new List<NextOfKinNotificationDelivery>();
        var contactDeliveries = new List<EmergencyAlertContactDelivery>();

        foreach (var result in deliveryResults)
        {
            contactDeliveries.Add(EmergencyAlertContactDelivery.Create(
                alert.Id,
                result.NextOfKinContactId,
                result.SmsStatus,
                result.PushStatus));

            if (result.SmsStatus == EmergencyAlertChannelDeliveryStatus.Failed)
            {
                retryDeliveries.Add(CreateRetryDelivery(
                    alert,
                    patient.Id,
                    result.NextOfKinContactId,
                    NextOfKinNotificationChannel.Sms,
                    nowUtc));
            }

            if (result.PushStatus == EmergencyAlertChannelDeliveryStatus.Failed)
            {
                retryDeliveries.Add(CreateRetryDelivery(
                    alert,
                    patient.Id,
                    result.NextOfKinContactId,
                    NextOfKinNotificationChannel.Push,
                    nowUtc));
            }
        }

        if (retryDeliveries.Count > 0)
        {
            await notificationDeliveryRepository.AddRangeAsync(retryDeliveries, ct);
            await notificationDeliveryRepository.SaveChangesAsync(ct);
            logger.LogInformation(
                "Emergency alert {AlertId} scheduled {RetryCount} next-of-kin channel retry delivery record(s).",
                alert.Id,
                retryDeliveries.Count);
        }

        return contactDeliveries;
    }

    private static NextOfKinNotificationDelivery CreateRetryDelivery(
        EmergencyAlert alert,
        Guid patientId,
        Guid contactId,
        NextOfKinNotificationChannel channel,
        DateTime failedAtUtc) =>
        NextOfKinNotificationDelivery.CreateAwaitingRetry(
            NextOfKinNotificationType.EmergencyAlert,
            alert.Id,
            patientId,
            contactId,
            channel,
            failedAtUtc,
            NextOfKinPolicies.NotificationRetryInterval);
}
