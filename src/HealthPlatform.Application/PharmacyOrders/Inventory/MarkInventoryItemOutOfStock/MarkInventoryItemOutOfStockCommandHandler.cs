using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.PharmacyOrders;
using MediatR;

namespace HealthPlatform.Application.PharmacyOrders.Inventory.MarkInventoryItemOutOfStock;

public sealed class MarkInventoryItemOutOfStockCommandHandler(
    ICurrentUserAccessor currentUser,
    IPharmacyRepository pharmacyRepository,
    IInventoryItemRepository inventoryItemRepository,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher)
    : IRequestHandler<MarkInventoryItemOutOfStockCommand, InventoryItemDto>
{
    public async Task<InventoryItemDto> Handle(MarkInventoryItemOutOfStockCommand request, CancellationToken ct)
    {
        var pharmacy = await MedicationOrderWorkflowSupport.ResolveCurrentPharmacyAsync(
            currentUser,
            pharmacyRepository,
            ct);

        var item = await inventoryItemRepository.GetByIdForPharmacyAsync(request.InventoryItemId, pharmacy.Id, ct)
            ?? throw new NotFoundException(
                InventoryErrorCodes.ItemNotFound,
                "Inventory item was not found.");

        var previousQuantity = item.Quantity;
        item.MarkOutOfStock();
        await inventoryItemRepository.UpdateAsync(item, ct);

        await InventoryLowStockAlertPublisher.PublishIfNeededAsync(
            item,
            previousQuantity,
            outboxRepository,
            domainEventPublisher,
            ct);

        var allItems = await inventoryItemRepository.ListByPharmacyIdAsync(pharmacy.Id, ct);
        await InventoryStockChangePublisher.PublishStockSummaryAsync(
            pharmacy.Id,
            allItems,
            outboxRepository,
            domainEventPublisher,
            ct);

        return item.ToDto();
    }
}
