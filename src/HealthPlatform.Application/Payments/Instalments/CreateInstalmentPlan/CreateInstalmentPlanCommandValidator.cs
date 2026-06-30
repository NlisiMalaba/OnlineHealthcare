using FluentValidation;
using HealthPlatform.Domain.Payments.Instalments;

namespace HealthPlatform.Application.Payments.Instalments.CreateInstalmentPlan;

public sealed class CreateInstalmentPlanCommandValidator : AbstractValidator<CreateInstalmentPlanCommand>
{
    public CreateInstalmentPlanCommandValidator()
    {
        RuleFor(x => x.TotalAmountMinorUnits).GreaterThan(0);
        RuleFor(x => x.InstalmentCount).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Frequency).IsInEnum();

        RuleFor(x => x)
            .Must(command =>
            {
                var targets = new[] { command.AppointmentId, command.MedicationOrderId, command.LabOrderId };
                return targets.Count(id => id is not null) == 1;
            })
            .WithMessage("Exactly one expense target id is required.");
    }
}
