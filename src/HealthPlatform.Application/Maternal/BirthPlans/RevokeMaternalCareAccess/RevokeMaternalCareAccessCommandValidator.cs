using FluentValidation;

namespace HealthPlatform.Application.Maternal.BirthPlans.RevokeMaternalCareAccess;

public sealed class RevokeMaternalCareAccessCommandValidator : AbstractValidator<RevokeMaternalCareAccessCommand>
{
    public RevokeMaternalCareAccessCommandValidator()
    {
        RuleFor(command => command.AntenatalRecordId)
            .NotEmpty();

        RuleFor(command => command.DoctorId)
            .NotEmpty();
    }
}
