using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Pharmacy;
using HealthPlatform.Domain.Pharmacy.Events;

namespace HealthPlatform.Application.PharmacyOrders.Inventory;

internal static class InventoryLowStockAlertPublisher
{
    public static async Task PublishIfNeededAsync(
        InventoryItem item,
        int previousQuantity,
        IOutboxRepository outboxRepository,
        IDomainEventPublisher domainEventPublisher,
        CancellationToken ct)
    {
        if (!InventoryPolicies.ShouldRaiseLowStockAlert(
                previousQuantity,
                item.Quantity,
                item.LowStockThreshold))
        {
            return;
        }

        var domainEvent = new InventoryLowStockDetectedDomainEvent(
            item.Id,
            item.PharmacyId,
            item.MedicationSku,
            item.MedicationName,
            item.Quantity,
            item.LowStockThreshold);

        await outboxRepository.EnqueueAsync(domainEvent, ct);
        await domainEventPublisher.PublishAsync(domainEvent, ct);
    }
}
