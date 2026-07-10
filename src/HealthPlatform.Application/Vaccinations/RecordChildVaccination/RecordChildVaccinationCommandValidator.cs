using FluentValidation;
using HealthPlatform.Application.Vaccinations;

namespace HealthPlatform.Application.Vaccinations.RecordChildVaccination;

public sealed class RecordChildVaccinationCommandValidator : AbstractValidator<RecordChildVaccinationCommand>
{
    public RecordChildVaccinationCommandValidator()
    {
        RuleFor(command => command.ChildProfileId).NotEmpty();
        RuleFor(command => command.VaccineName)
            .NotEmpty()
            .MaximumLength(VaccinationPolicies.MaxVaccineNameLength);
        RuleFor(command => command.AdministeredDate).NotEmpty();
        RuleFor(command => command.BatchNumber)
            .NotEmpty()
            .MaximumLength(VaccinationPolicies.MaxBatchNumberLength);
        RuleFor(command => command.Provider)
            .NotEmpty()
            .MaximumLength(VaccinationPolicies.MaxProviderLength);
    }
}
