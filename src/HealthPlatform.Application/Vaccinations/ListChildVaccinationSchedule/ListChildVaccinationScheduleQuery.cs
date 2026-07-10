using HealthPlatform.Application.Vaccinations;
using MediatR;

namespace HealthPlatform.Application.Vaccinations.ListChildVaccinationSchedule;

public sealed record ListChildVaccinationScheduleQuery(Guid ChildProfileId)
    : IRequest<IReadOnlyList<VaccinationScheduleEntryDto>>;
