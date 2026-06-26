using FluentValidation;

namespace HealthPlatform.Application.PharmacyOrders.Inventory.AddInventoryItem;

public sealed class AddInventoryItemCommandValidator : AbstractValidator<AddInventoryItemCommand>
{
    public AddInventoryItemCommandValidator()
    {
        RuleFor(x => x.MedicationName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MedicationSku).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LowStockThreshold)
            .GreaterThanOrEqualTo(0)
            .When(x => x.LowStockThreshold.HasValue);
    }
}
