using HealthPlatform.Application.Identity.Notifications;
using HealthPlatform.Application.Search;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Search.EventHandlers;

public sealed class DoctorLicenseRejectedSearchIndexHandler(
    ISearchService searchService,
    ILogger<DoctorLicenseRejectedSearchIndexHandler> logger) : INotificationHandler<DoctorLicenseRejectedNotification>
{
    public async Task Handle(DoctorLicenseRejectedNotification notification, CancellationToken ct)
    {
        await searchService.RemoveDoctorAsync(notification.DoctorId, ct);

        logger.LogInformation(
            "Removed rejected doctor {DoctorId} from search index.",
            notification.DoctorId);
    }
}
