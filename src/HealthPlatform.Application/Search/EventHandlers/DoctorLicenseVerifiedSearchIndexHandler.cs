using HealthPlatform.Application.Identity.Notifications;
using HealthPlatform.Application.Search;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Search.EventHandlers;

public sealed class DoctorLicenseVerifiedSearchIndexHandler(
    ISearchService searchService,
    ILogger<DoctorLicenseVerifiedSearchIndexHandler> logger) : INotificationHandler<DoctorLicenseVerifiedNotification>
{
    public async Task Handle(DoctorLicenseVerifiedNotification notification, CancellationToken ct)
    {
        await searchService.UpsertDoctorAsync(notification.DoctorId, ct);

        logger.LogInformation(
            "Indexed verified doctor {DoctorId} in search index.",
            notification.DoctorId);
    }
}
