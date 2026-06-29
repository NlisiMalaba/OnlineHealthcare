using FluentValidation;

namespace HealthPlatform.Application.PharmacyOrders.ConfirmMedicationOrder;

public sealed class ConfirmMedicationOrderCommandValidator : AbstractValidator<ConfirmMedicationOrderCommand>
{
    public ConfirmMedicationOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
