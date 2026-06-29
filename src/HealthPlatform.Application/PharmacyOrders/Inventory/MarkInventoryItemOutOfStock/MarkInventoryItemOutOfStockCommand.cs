using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.PharmacyOrders.Inventory.MarkInventoryItemOutOfStock;

public sealed record MarkInventoryItemOutOfStockCommand(Guid InventoryItemId) : ICommand<InventoryItemDto>;
