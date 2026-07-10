using FluentValidation;
using HealthPlatform.Application.Vaccinations;

namespace HealthPlatform.Application.Vaccinations.RecordPatientVaccination;

public sealed class RecordPatientVaccinationCommandValidator : AbstractValidator<RecordPatientVaccinationCommand>
{
    public RecordPatientVaccinationCommandValidator()
    {
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
