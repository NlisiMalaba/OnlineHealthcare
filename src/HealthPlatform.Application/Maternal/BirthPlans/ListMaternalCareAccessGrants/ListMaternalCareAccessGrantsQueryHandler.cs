using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Maternal.AntenatalRecords;
using MediatR;

namespace HealthPlatform.Application.Maternal.BirthPlans.ListMaternalCareAccessGrants;

public sealed class ListMaternalCareAccessGrantsQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IAntenatalRecordRepository antenatalRecordRepository,
    IMaternalCareAccessRepository maternalCareAccessRepository)
    : IRequestHandler<ListMaternalCareAccessGrantsQuery, IReadOnlyList<MaternalCareAccessGrantDto>>
{
    public async Task<IReadOnlyList<MaternalCareAccessGrantDto>> Handle(
        ListMaternalCareAccessGrantsQuery request,
        CancellationToken ct)
    {
        var antenatalRecord = await antenatalRecordRepository.GetByIdAsync(request.AntenatalRecordId, ct)
            ?? throw new NotFoundException(
                BirthPlanErrorCodes.AntenatalRecordNotFound,
                "Antenatal record was not found.");

        var patient = await ResolvePatientAsync(ct);
        if (antenatalRecord.PatientId != patient.Id)
        {
            throw new AccessDeniedException(
                BirthPlanErrorCodes.AccessDenied,
                "You can only list access grants for your own maternal care records.");
        }

        var grants = await maternalCareAccessRepository.ListActiveGrantsByAntenatalRecordIdAsync(
            antenatalRecord.Id,
            ct);

        var results = new List<MaternalCareAccessGrantDto>(grants.Count);
        foreach (var grant in grants)
        {
            var doctor = await doctorRepository.GetByIdAsync(grant.DoctorId, ct);
            results.Add(grant.ToDto(doctor?.FullName ?? "Unknown doctor"));
        }

        return results;
    }

    private async Task<Domain.Identity.Patient> ResolvePatientAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(
                BirthPlanErrorCodes.PatientNotFound,
                "Patient profile was not found.");
    }
}
