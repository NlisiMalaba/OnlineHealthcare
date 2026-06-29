using FluentValidation;
using HealthPlatform.Domain.Pharmacy;

namespace HealthPlatform.Application.PharmacyOrders.CreateMedicationOrder;

public sealed class CreateMedicationOrderCommandValidator : AbstractValidator<CreateMedicationOrderCommand>
{
    public CreateMedicationOrderCommandValidator()
    {
        RuleFor(x => x.PrescriptionId).NotEmpty();
        RuleFor(x => x.PharmacyId).NotEmpty();
        RuleFor(x => x.MedicationSku)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(x => x.DeliveryType).IsInEnum();

        RuleFor(x => x.DeliveryAddress)
            .NotEmpty()
            .MaximumLength(500)
            .When(x => x.DeliveryType == MedicationDeliveryType.Delivery);

        RuleFor(x => x.DeliveryAddress)
            .Null()
            .When(x => x.DeliveryType == MedicationDeliveryType.Pickup);
    }
}
