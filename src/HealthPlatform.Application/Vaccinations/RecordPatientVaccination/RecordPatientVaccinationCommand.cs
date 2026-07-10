using HealthPlatform.Application.Vaccinations;
using MediatR;

namespace HealthPlatform.Application.Vaccinations.RecordPatientVaccination;

public sealed record RecordPatientVaccinationCommand(
    Guid? PatientId,
    Guid? ScheduleEntryId,
    string VaccineName,
    DateOnly AdministeredDate,
    string BatchNumber,
    string Provider) : IRequest<VaccinationRecordDto>;
