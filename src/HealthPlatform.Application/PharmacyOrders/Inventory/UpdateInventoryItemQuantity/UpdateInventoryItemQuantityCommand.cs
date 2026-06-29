using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.PharmacyOrders.Inventory.UpdateInventoryItemQuantity;

public sealed record UpdateInventoryItemQuantityCommand(
    Guid InventoryItemId,
    int Quantity) : ICommand<InventoryItemDto>;
