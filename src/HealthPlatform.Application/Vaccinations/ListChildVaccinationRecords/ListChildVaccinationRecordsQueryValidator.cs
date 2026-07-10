using FluentValidation;

namespace HealthPlatform.Application.Vaccinations.ListChildVaccinationRecords;

public sealed class ListChildVaccinationRecordsQueryValidator
    : AbstractValidator<ListChildVaccinationRecordsQuery>
{
    public ListChildVaccinationRecordsQueryValidator()
    {
        RuleFor(query => query.ChildProfileId).NotEmpty();
    }
}
