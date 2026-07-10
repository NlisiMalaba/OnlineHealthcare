using FluentValidation;

namespace HealthPlatform.Application.Vaccinations.ListChildVaccinationSchedule;

public sealed class ListChildVaccinationScheduleQueryValidator
    : AbstractValidator<ListChildVaccinationScheduleQuery>
{
    public ListChildVaccinationScheduleQueryValidator()
    {
        RuleFor(query => query.ChildProfileId).NotEmpty();
    }
}
