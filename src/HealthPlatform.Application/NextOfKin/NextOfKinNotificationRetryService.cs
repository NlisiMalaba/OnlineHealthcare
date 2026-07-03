using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.NextOfKin;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.NextOfKin;

public sealed class NextOfKinNotificationRetryService(
    TimeProvider timeProvider,
    INextOfKinNotificationDeliveryRepository notificationDeliveryRepository,
    INextOfKinChannelDeliveryGateway channelDeliveryGateway,
    IEmergencyAlertRepository emergencyAlertRepository,
    INextOfKinRepository nextOfKinRepository,
    IPatientRepository patientRepository,
    ILogger<NextOfKinNotificationRetryService> logger) : INextOfKinNotificationRetryService
{
    public async Task<int> ProcessDueRetriesAsync(CancellationToken ct)
    {
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var dueDeliveries = await notificationDeliveryRepository.ListDueForRetryAsync(
            nowUtc,
            NextOfKinPolicies.NotificationRetryBatchSize,
            ct);

        var processed = 0;
        foreach (var delivery in dueDeliveries)
        {
            if (delivery.RetryCount >= NextOfKinPolicies.MaxNotificationRetries)
            {
                continue;
            }

            var succeeded = await AttemptRetryAsync(delivery, nowUtc, ct);
            if (succeeded)
            {
                delivery.RecordSuccessfulAttempt(nowUtc);
            }
            else
            {
                delivery.RecordFailedRetryAttempt(
                    nowUtc,
                    NextOfKinPolicies.MaxNotificationRetries,
                    NextOfKinPolicies.NotificationRetryInterval);
            }

            await notificationDeliveryRepository.UpdateAsync(delivery, ct);
            await UpdateEmergencyAlertAggregateIfNeededAsync(delivery, ct);
            processed++;

            if (delivery.Status == NextOfKinNotificationDeliveryStatus.FailedFinal)
            {
                logger.LogWarning(
                    "Next-of-kin {NotificationType} delivery {DeliveryId} for contact {ContactId} on channel {Channel} failed after {RetryCount} retry attempt(s).",
                    delivery.NotificationType,
                    delivery.Id,
                    delivery.NextOfKinContactId,
                    delivery.Channel,
                    delivery.RetryCount);
            }
        }

        if (processed > 0)
        {
            await notificationDeliveryRepository.SaveChangesAsync(ct);
            await emergencyAlertRepository.SaveChangesAsync(ct);
        }

        return processed;
    }

    private async Task<bool> AttemptRetryAsync(
        NextOfKinNotificationDelivery delivery,
        DateTime attemptedAtUtc,
        CancellationToken ct)
    {
        return delivery.NotificationType switch
        {
            NextOfKinNotificationType.EmergencyAlert => await AttemptEmergencyAlertRetryAsync(delivery, ct),
            _ => false
        };
    }

    private async Task<bool> AttemptEmergencyAlertRetryAsync(
        NextOfKinNotificationDelivery delivery,
        CancellationToken ct)
    {
        var alert = await emergencyAlertRepository.GetByIdAsync(delivery.ReferenceId, ct);
        if (alert is null)
        {
            logger.LogWarning(
                "Skipping retry for delivery {DeliveryId}; emergency alert {AlertId} was not found.",
                delivery.Id,
                delivery.ReferenceId);
            return false;
        }

        var patient = await patientRepository.GetByIdAsync(delivery.PatientId, ct);
        if (patient is null)
        {
            return false;
        }

        var contact = await nextOfKinRepository.GetByIdForPatientAsync(
            delivery.NextOfKinContactId,
            delivery.PatientId,
            ct);
        if (contact is null)
        {
            return false;
        }

        return await channelDeliveryGateway.TryDeliverEmergencyAlertAsync(
            new EmergencyAlertChannelDeliveryRequest(
                alert.Id,
                patient.Id,
                patient.FullName,
                alert.TriggerReason,
                contact.ToDto(),
                delivery.Channel),
            ct);
    }

    private async Task UpdateEmergencyAlertAggregateIfNeededAsync(
        NextOfKinNotificationDelivery delivery,
        CancellationToken ct)
    {
        if (delivery.NotificationType != NextOfKinNotificationType.EmergencyAlert)
        {
            return;
        }

        var contactDelivery = await emergencyAlertRepository.GetContactDeliveryAsync(
            delivery.ReferenceId,
            delivery.NextOfKinContactId,
            ct);
        if (contactDelivery is null)
        {
            return;
        }

        var channelStatus = MapDeliveryStatus(delivery.Status);
        if (delivery.Channel == NextOfKinNotificationChannel.Sms)
        {
            contactDelivery.RecordSmsStatus(channelStatus);
        }
        else
        {
            contactDelivery.RecordPushStatus(channelStatus);
        }

        await emergencyAlertRepository.UpdateContactDeliveryAsync(contactDelivery, ct);
        await emergencyAlertRepository.UpdateOverallStatusAsync(delivery.ReferenceId, ct);
    }

    private static EmergencyAlertChannelDeliveryStatus MapDeliveryStatus(
        NextOfKinNotificationDeliveryStatus status) =>
        status switch
        {
            NextOfKinNotificationDeliveryStatus.Sent => EmergencyAlertChannelDeliveryStatus.Sent,
            _ => EmergencyAlertChannelDeliveryStatus.Failed
        };
}
