using FluentValidation;

namespace HealthPlatform.Application.PharmacyOrders.Inventory.MarkInventoryItemOutOfStock;

public sealed class MarkInventoryItemOutOfStockCommandValidator
    : AbstractValidator<MarkInventoryItemOutOfStockCommand>
{
    public MarkInventoryItemOutOfStockCommandValidator()
    {
        RuleFor(x => x.InventoryItemId).NotEmpty();
    }
}
