using FluentValidation.TestHelper;
using HealthPlatform.Application.PharmacyOrders.Inventory.AddInventoryItem;
using Xunit;

namespace HealthPlatform.Tests.Unit.PharmacyOrders;

public sealed class AddInventoryItemCommandValidatorTests
{
    private readonly AddInventoryItemCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidInput_PassesValidation()
    {
        var result = _validator.TestValidate(new AddInventoryItemCommand("Paracetamol", "MED-001", 10, 5));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNegativeQuantity_FailsValidation()
    {
        var result = _validator.TestValidate(new AddInventoryItemCommand("Paracetamol", "MED-001", -1, null));
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }
}
