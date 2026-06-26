using FluentValidation.TestHelper;
using HealthPlatform.Application.PharmacyOrders.CreateMedicationOrder;
using HealthPlatform.Domain.Pharmacy;
using Xunit;

namespace HealthPlatform.Tests.Unit.PharmacyOrders;

public sealed class CreateMedicationOrderCommandValidatorTests
{
    private readonly CreateMedicationOrderCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidDeliveryOrder_PassesValidation()
    {
        var result = _validator.TestValidate(new CreateMedicationOrderCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "MED-001",
            MedicationDeliveryType.Delivery,
            "12 Samora Machel Ave, Harare"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithPickupOrderWithoutAddress_PassesValidation()
    {
        var result = _validator.TestValidate(new CreateMedicationOrderCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "MED-001",
            MedicationDeliveryType.Pickup,
            null));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithDeliveryOrderMissingAddress_FailsValidation()
    {
        var result = _validator.TestValidate(new CreateMedicationOrderCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "MED-001",
            MedicationDeliveryType.Delivery,
            null));

        result.ShouldHaveValidationErrorFor(x => x.DeliveryAddress);
    }

    [Fact]
    public void Validate_WithPickupOrderAndAddress_FailsValidation()
    {
        var result = _validator.TestValidate(new CreateMedicationOrderCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "MED-001",
            MedicationDeliveryType.Pickup,
            "12 Samora Machel Ave, Harare"));

        result.ShouldHaveValidationErrorFor(x => x.DeliveryAddress);
    }
}
