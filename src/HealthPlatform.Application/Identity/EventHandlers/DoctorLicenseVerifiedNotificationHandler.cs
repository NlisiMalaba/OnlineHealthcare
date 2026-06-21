using HealthPlatform.Application.Identity.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Identity.EventHandlers;

public sealed class DoctorLicenseVerifiedNotificationHandler(
    IDoctorLicenseVerificationNotifier notifier,
    ILogger<DoctorLicenseVerifiedNotificationHandler> logger) : INotificationHandler<DoctorLicenseVerifiedNotification>
{
    public async Task Handle(DoctorLicenseVerifiedNotification notification, CancellationToken ct)
    {
        logger.LogInformation(
            "License verified notification dispatch for doctor {DoctorId}.",
            notification.DoctorId);

        await notifier.NotifyLicenseVerifiedAsync(
            notification.UserId,
            notification.DoctorId,
            notification.FullName,
            ct);
    }
}
