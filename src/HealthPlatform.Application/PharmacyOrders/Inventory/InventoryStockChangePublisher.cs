using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Identity.Events;
using HealthPlatform.Domain.Pharmacy;

namespace HealthPlatform.Application.PharmacyOrders.Inventory;

internal static class InventoryStockChangePublisher
{
    public static async Task PublishStockSummaryAsync(
        Guid pharmacyId,
        IReadOnlyList<InventoryItem> items,
        IOutboxRepository outboxRepository,
        IDomainEventPublisher domainEventPublisher,
        CancellationToken ct)
    {
        var stockSummary = items
            .Select(item => new PharmacyStockSummaryItem(
                item.MedicationName,
                item.MedicationSku,
                item.Quantity))
            .ToList();

        var domainEvent = new PharmacyStockChangedDomainEvent(pharmacyId, stockSummary);
        await outboxRepository.EnqueueAsync(domainEvent, ct);
        await domainEventPublisher.PublishAsync(domainEvent, ct);
    }
}
