using HealthPlatform.Application.Vaccinations;
using MediatR;

namespace HealthPlatform.Application.Vaccinations.ListPatientVaccinationRecords;

public sealed record ListPatientVaccinationRecordsQuery : IRequest<IReadOnlyList<VaccinationRecordDto>>;
