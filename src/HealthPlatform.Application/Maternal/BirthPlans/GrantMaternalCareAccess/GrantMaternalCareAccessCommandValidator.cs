using FluentValidation;

namespace HealthPlatform.Application.Maternal.BirthPlans.GrantMaternalCareAccess;

public sealed class GrantMaternalCareAccessCommandValidator : AbstractValidator<GrantMaternalCareAccessCommand>
{
    public GrantMaternalCareAccessCommandValidator()
    {
        RuleFor(command => command.AntenatalRecordId)
            .NotEmpty();

        RuleFor(command => command.DoctorId)
            .NotEmpty();

        RuleFor(command => command)
            .Must(command => command.ShareAntenatalRecord || command.ShareBirthPlan)
            .WithMessage("At least one of antenatal record or birth plan sharing must be enabled.");
    }
}
