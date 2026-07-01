using FluentValidation;

namespace HealthPlatform.Application.Wellness.ConfirmMedicationDose;

public sealed class ConfirmMedicationDoseCommandValidator : AbstractValidator<ConfirmMedicationDoseCommand>
{
    public ConfirmMedicationDoseCommandValidator()
    {
        RuleFor(command => command.ScheduleId)
            .NotEmpty();

        RuleFor(command => command.ScheduledAtUtc)
            .Must(scheduledAt => scheduledAt.Kind == DateTimeKind.Utc)
            .WithMessage("Scheduled dose time must be UTC.");
    }
}
