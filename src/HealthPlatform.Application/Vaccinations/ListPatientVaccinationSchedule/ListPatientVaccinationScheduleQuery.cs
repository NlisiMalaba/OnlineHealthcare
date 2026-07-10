using HealthPlatform.Application.Vaccinations;
using MediatR;

namespace HealthPlatform.Application.Vaccinations.ListPatientVaccinationSchedule;

public sealed record ListPatientVaccinationScheduleQuery : IRequest<IReadOnlyList<VaccinationScheduleEntryDto>>;
