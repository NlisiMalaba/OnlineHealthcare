using FluentValidation;

namespace HealthPlatform.Application.Payments.CreditLine.PayOnCreditLine;

public sealed class PayOnCreditLineCommandValidator : AbstractValidator<PayOnCreditLineCommand>
{
    public PayOnCreditLineCommandValidator()
    {
        RuleFor(x => x.AmountMinorUnits).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);

        RuleFor(x => x)
            .Must(command =>
            {
                var targets = new[] { command.AppointmentId, command.MedicationOrderId, command.LabOrderId };
                return targets.Count(id => id is not null) == 1;
            })
            .WithMessage("Exactly one payment target id is required.");
    }
}
