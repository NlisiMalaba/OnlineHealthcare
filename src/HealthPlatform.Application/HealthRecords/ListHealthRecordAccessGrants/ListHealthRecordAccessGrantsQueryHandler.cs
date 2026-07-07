using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.Identity;
using MediatR;

namespace HealthPlatform.Application.HealthRecords.ListHealthRecordAccessGrants;

public sealed class ListHealthRecordAccessGrantsQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IHealthRecordRepository healthRecordRepository,
    IHealthRecordAccessRepository healthRecordAccessRepository)
    : IRequestHandler<ListHealthRecordAccessGrantsQuery, IReadOnlyList<HealthRecordAccessDto>>
{
    public async Task<IReadOnlyList<HealthRecordAccessDto>> Handle(
        ListHealthRecordAccessGrantsQuery request,
        CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var healthRecord = await healthRecordRepository.GetByPatientIdAsync(patient.Id, ct)
            ?? throw new NotFoundException(
                HealthRecordErrorCodes.HealthRecordNotFound,
                "Health record was not found.");

        var grants = await healthRecordAccessRepository.ListByHealthRecordIdAsync(healthRecord.Id, ct);
        var doctorNames = new Dictionary<Guid, string>();
        var result = new List<HealthRecordAccessDto>(grants.Count);

        foreach (var grant in grants)
        {
            if (!doctorNames.TryGetValue(grant.GrantedToDoctorId, out var doctorName))
            {
                var doctor = await doctorRepository.GetByIdAsync(grant.GrantedToDoctorId, ct);
                doctorName = doctor?.FullName ?? "Unknown doctor";
                doctorNames[grant.GrantedToDoctorId] = doctorName;
            }

            result.Add(grant.ToDto(doctorName));
        }

        return result;
    }

    private async Task<Patient> ResolvePatientAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(
                HealthRecordErrorCodes.PatientNotFound,
                "Patient profile was not found.");
    }
}
