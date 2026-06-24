using FluentValidation;

namespace HealthPlatform.Application.Prescriptions.CreatePrescription;

public sealed class CreatePrescriptionCommandValidator : AbstractValidator<CreatePrescriptionCommand>
{
    public CreatePrescriptionCommandValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty();

        RuleFor(x => x.MedicationName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Dosage)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Frequency)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.DurationDays)
            .GreaterThan(0)
            .LessThanOrEqualTo(365);

        RuleFor(x => x.SpecialInstructions)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.SpecialInstructions));

        RuleFor(x => x.ExpiresAtUtc)
            .Must(expiresAt => expiresAt!.Value.Kind == DateTimeKind.Utc)
            .When(x => x.ExpiresAtUtc.HasValue)
            .WithMessage("Expiry time must be UTC.");
    }
}
