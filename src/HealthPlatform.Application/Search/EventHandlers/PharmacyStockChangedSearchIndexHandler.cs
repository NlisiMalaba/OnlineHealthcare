using HealthPlatform.Application.Search;
using HealthPlatform.Application.Search.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Search.EventHandlers;

public sealed class PharmacyStockChangedSearchIndexHandler(
    ISearchService searchService,
    ILogger<PharmacyStockChangedSearchIndexHandler> logger) : INotificationHandler<PharmacyStockChangedNotification>
{
    public async Task Handle(PharmacyStockChangedNotification notification, CancellationToken ct)
    {
        var stock = notification.StockSummary
            .Select(item => new PharmacyStockIndexEntry(
                item.MedicationName,
                item.MedicationSku,
                item.QuantityOnHand))
            .ToList();

        await searchService.UpdatePharmacyStockAsync(notification.PharmacyId, stock, ct);

        logger.LogInformation(
            "Synchronized pharmacy {PharmacyId} stock to search index.",
            notification.PharmacyId);
    }
}
