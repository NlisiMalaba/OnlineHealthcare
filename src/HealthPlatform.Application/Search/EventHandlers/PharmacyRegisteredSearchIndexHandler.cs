using HealthPlatform.Application.Search;
using HealthPlatform.Application.Search.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Search.EventHandlers;

public sealed class PharmacyRegisteredSearchIndexHandler(
    ISearchService searchService,
    ILogger<PharmacyRegisteredSearchIndexHandler> logger) : INotificationHandler<PharmacyRegisteredSearchNotification>
{
    public async Task Handle(PharmacyRegisteredSearchNotification notification, CancellationToken ct)
    {
        await searchService.UpsertPharmacyAsync(notification.PharmacyId, ct);

        logger.LogInformation(
            "Indexed pharmacy {PharmacyId} in search index.",
            notification.PharmacyId);
    }
}
