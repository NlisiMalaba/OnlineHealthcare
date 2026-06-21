using HealthPlatform.Application.Identity.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Identity.EventHandlers;

public sealed class DoctorLicenseRejectedNotificationHandler(
    IDoctorLicenseVerificationNotifier notifier,
    ILogger<DoctorLicenseRejectedNotificationHandler> logger) : INotificationHandler<DoctorLicenseRejectedNotification>
{
    public async Task Handle(DoctorLicenseRejectedNotification notification, CancellationToken ct)
    {
        logger.LogInformation(
            "License rejected notification dispatch for doctor {DoctorId}.",
            notification.DoctorId);

        await notifier.NotifyLicenseRejectedAsync(
            notification.UserId,
            notification.DoctorId,
            notification.FullName,
            notification.Reason,
            ct);
    }
}
