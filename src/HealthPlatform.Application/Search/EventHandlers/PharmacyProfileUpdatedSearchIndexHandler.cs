using HealthPlatform.Application.Search;
using HealthPlatform.Application.Search.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Search.EventHandlers;

public sealed class PharmacyProfileUpdatedSearchIndexHandler(
    ISearchService searchService,
    ILogger<PharmacyProfileUpdatedSearchIndexHandler> logger) : INotificationHandler<PharmacyProfileUpdatedNotification>
{
    public async Task Handle(PharmacyProfileUpdatedNotification notification, CancellationToken ct)
    {
        await searchService.UpsertPharmacyAsync(notification.PharmacyId, ct);

        logger.LogInformation(
            "Synchronized pharmacy {PharmacyId} profile to search index.",
            notification.PharmacyId);
    }
}
