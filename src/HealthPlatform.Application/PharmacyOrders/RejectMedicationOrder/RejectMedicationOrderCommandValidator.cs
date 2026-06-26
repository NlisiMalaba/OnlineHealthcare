using FluentValidation;

namespace HealthPlatform.Application.PharmacyOrders.RejectMedicationOrder;

public sealed class RejectMedicationOrderCommandValidator : AbstractValidator<RejectMedicationOrderCommand>
{
    public RejectMedicationOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
