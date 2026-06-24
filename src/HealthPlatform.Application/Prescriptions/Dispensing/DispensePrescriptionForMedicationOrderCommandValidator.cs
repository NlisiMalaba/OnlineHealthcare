using FluentValidation;

namespace HealthPlatform.Application.Prescriptions.Dispensing;

public sealed class DispensePrescriptionForMedicationOrderCommandValidator
    : AbstractValidator<DispensePrescriptionForMedicationOrderCommand>
{
    public DispensePrescriptionForMedicationOrderCommandValidator()
    {
        RuleFor(x => x.PrescriptionId)
            .NotEmpty();
    }
}
