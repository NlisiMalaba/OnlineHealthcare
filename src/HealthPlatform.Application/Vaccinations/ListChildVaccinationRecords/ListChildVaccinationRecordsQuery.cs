using HealthPlatform.Application.Vaccinations;
using MediatR;

namespace HealthPlatform.Application.Vaccinations.ListChildVaccinationRecords;

public sealed record ListChildVaccinationRecordsQuery(Guid ChildProfileId)
    : IRequest<IReadOnlyList<VaccinationRecordDto>>;
