using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Application.Vaccinations;
using MediatR;

namespace HealthPlatform.Application.Vaccinations.ListPatientVaccinationRecords;

public sealed class ListPatientVaccinationRecordsQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IVaccinationRecordRepository recordRepository)
    : IRequestHandler<ListPatientVaccinationRecordsQuery, IReadOnlyList<VaccinationRecordDto>>
{
    public async Task<IReadOnlyList<VaccinationRecordDto>> Handle(
        ListPatientVaccinationRecordsQuery request,
        CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var records = await recordRepository.ListByPatientIdAsync(patient.Id, ct);
        return records.Select(record => record.ToDto()).ToList();
    }

    private async Task<Domain.Identity.Patient> ResolvePatientAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(
                VaccinationErrorCodes.PatientNotFound,
                "Patient profile was not found.");
    }
}
