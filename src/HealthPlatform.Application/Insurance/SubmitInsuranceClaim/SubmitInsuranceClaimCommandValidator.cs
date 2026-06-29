using FluentValidation;
using HealthPlatform.Domain.Insurance;

namespace HealthPlatform.Application.Insurance.SubmitInsuranceClaim;

public sealed class SubmitInsuranceClaimCommandValidator : AbstractValidator<SubmitInsuranceClaimCommand>
{
    public SubmitInsuranceClaimCommandValidator()
    {
        RuleFor(x => x.InsurerCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.AmountMinorUnits).GreaterThan(0);
        RuleFor(x => x.ClaimType).IsInEnum();

        RuleFor(x => x)
            .Must(command =>
            {
                var targets = new[] { command.AppointmentId, command.MedicationOrderId, command.LabOrderId };
                return targets.Count(id => id is not null) == 1;
            })
            .WithMessage("Exactly one claim target id is required.");

        RuleFor(x => x)
            .Must(command => command.ClaimType switch
            {
                InsuranceClaimType.Consultation => command.AppointmentId is not null,
                InsuranceClaimType.Medication => command.MedicationOrderId is not null,
                InsuranceClaimType.LabTest => command.LabOrderId is not null,
                _ => false
            })
            .WithMessage("Claim type does not match the provided target id.");
    }
}
