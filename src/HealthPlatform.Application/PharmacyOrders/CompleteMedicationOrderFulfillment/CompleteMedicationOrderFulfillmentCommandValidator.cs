using FluentValidation;

namespace HealthPlatform.Application.PharmacyOrders.CompleteMedicationOrderFulfillment;

public sealed class CompleteMedicationOrderFulfillmentCommandValidator
    : AbstractValidator<CompleteMedicationOrderFulfillmentCommand>
{
    public CompleteMedicationOrderFulfillmentCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
