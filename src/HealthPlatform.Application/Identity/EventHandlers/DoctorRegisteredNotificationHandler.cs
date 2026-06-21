using HealthPlatform.Application.Identity.Notifications;
using HealthPlatform.Domain.Identity;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Identity.EventHandlers;

public sealed class DoctorRegisteredNotificationHandler(
    ILicenseVerificationQueueRepository licenseVerificationQueueRepository,
    ILogger<DoctorRegisteredNotificationHandler> logger) : INotificationHandler<DoctorRegisteredNotification>
{
    public async Task Handle(DoctorRegisteredNotification notification, CancellationToken ct)
    {
        if (await licenseVerificationQueueRepository.ExistsPendingForDoctorAsync(notification.DoctorId, ct))
        {
            logger.LogDebug(
                "License verification queue item already exists for doctor {DoctorId}; skipping enqueue.",
                notification.DoctorId);
            return;
        }

        var queueItem = LicenseVerificationQueueItem.Create(notification.DoctorId);
        await licenseVerificationQueueRepository.EnqueueAsync(queueItem, ct);

        logger.LogInformation(
            "Queued license verification for doctor {DoctorId} with license {LicenseNumber}.",
            notification.DoctorId,
            notification.LicenseNumber);
    }
}
