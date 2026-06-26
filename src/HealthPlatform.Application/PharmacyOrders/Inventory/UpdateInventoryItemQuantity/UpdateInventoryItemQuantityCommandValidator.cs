using FluentValidation;

namespace HealthPlatform.Application.PharmacyOrders.Inventory.UpdateInventoryItemQuantity;

public sealed class UpdateInventoryItemQuantityCommandValidator
    : AbstractValidator<UpdateInventoryItemQuantityCommand>
{
    public UpdateInventoryItemQuantityCommandValidator()
    {
        RuleFor(x => x.InventoryItemId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
    }
}
