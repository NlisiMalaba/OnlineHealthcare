using HealthPlatform.Application.Vaccinations;
using MediatR;

namespace HealthPlatform.Application.Vaccinations.RecordChildVaccination;

public sealed record RecordChildVaccinationCommand(
    Guid ChildProfileId,
    Guid? ScheduleEntryId,
    string VaccineName,
    DateOnly AdministeredDate,
    string BatchNumber,
    string Provider) : IRequest<VaccinationRecordDto>;
