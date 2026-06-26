using FluentValidation;

namespace HealthPlatform.Application.PharmacyOrders.MarkMedicationOrderDispatched;

public sealed class MarkMedicationOrderDispatchedCommandValidator
    : AbstractValidator<MarkMedicationOrderDispatchedCommand>
{
    public MarkMedicationOrderDispatchedCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
