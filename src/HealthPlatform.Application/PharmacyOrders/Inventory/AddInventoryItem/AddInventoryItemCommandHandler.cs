using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.PharmacyOrders;
using HealthPlatform.Domain.Pharmacy;
using MediatR;

namespace HealthPlatform.Application.PharmacyOrders.Inventory.AddInventoryItem;

public sealed class AddInventoryItemCommandHandler(
    ICurrentUserAccessor currentUser,
    IPharmacyRepository pharmacyRepository,
    IInventoryItemRepository inventoryItemRepository,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher)
    : IRequestHandler<AddInventoryItemCommand, InventoryItemDto>
{
    public async Task<InventoryItemDto> Handle(AddInventoryItemCommand request, CancellationToken ct)
    {
        var pharmacy = await MedicationOrderWorkflowSupport.ResolveCurrentPharmacyAsync(
            currentUser,
            pharmacyRepository,
            ct);

        var medicationSku = request.MedicationSku.Trim();
        if (await inventoryItemRepository.ExistsByPharmacyAndSkuAsync(pharmacy.Id, medicationSku, ct))
        {
            throw new ConflictException(
                InventoryErrorCodes.SkuAlreadyExists,
                "An inventory item with this medication SKU already exists.");
        }

        var item = InventoryItem.Add(
            pharmacy.Id,
            request.MedicationName,
            medicationSku,
            request.Quantity,
            request.LowStockThreshold);

        await inventoryItemRepository.AddAsync(item, ct);

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
