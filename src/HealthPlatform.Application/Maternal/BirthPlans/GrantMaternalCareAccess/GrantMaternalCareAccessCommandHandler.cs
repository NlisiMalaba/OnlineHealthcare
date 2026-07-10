using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Maternal.AntenatalRecords;
using HealthPlatform.Domain.Maternal;
using MediatR;

namespace HealthPlatform.Application.Maternal.BirthPlans.GrantMaternalCareAccess;

public sealed class GrantMaternalCareAccessCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IAntenatalRecordRepository antenatalRecordRepository,
    IBirthPlanRepository birthPlanRepository,
    IMaternalCareAccessRepository maternalCareAccessRepository,
    TimeProvider timeProvider)
    : IRequestHandler<GrantMaternalCareAccessCommand, MaternalCareAccessGrantDto>
{
    public async Task<MaternalCareAccessGrantDto> Handle(GrantMaternalCareAccessCommand request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var antenatalRecord = await antenatalRecordRepository.GetByIdAsync(request.AntenatalRecordId, ct)
            ?? throw new NotFoundException(
                BirthPlanErrorCodes.AntenatalRecordNotFound,
                "Antenatal record was not found.");

        if (antenatalRecord.PatientId != patient.Id)
        {
            throw new AccessDeniedException(
                BirthPlanErrorCodes.AccessDenied,
                "You can only share your own maternal care records.");
        }

        if (request.ShareBirthPlan)
        {
            _ = await birthPlanRepository.GetByAntenatalRecordIdAsync(antenatalRecord.Id, ct)
                ?? throw new NotFoundException(
                    BirthPlanErrorCodes.BirthPlanNotFound,
                    "Birth plan must exist before it can be shared.");
        }

        var doctor = await doctorRepository.GetByIdAsync(request.DoctorId, ct)
            ?? throw new NotFoundException(
                BirthPlanErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");

        var activeGrant = await maternalCareAccessRepository.GetActiveGrantAsync(
            antenatalRecord.Id,
            doctor.Id,
            ct);
        if (activeGrant is not null)
        {
            throw new ConflictException(
                BirthPlanErrorCodes.AccessAlreadyGranted,
                "Doctor already has active access to this maternal care record.");
        }

        var grantedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var latestGrant = await maternalCareAccessRepository.GetLatestGrantAsync(
            antenatalRecord.Id,
            doctor.Id,
            ct);

        MaternalCareAccessGrant grant;
        if (latestGrant is null)
        {
            grant = MaternalCareAccessGrant.Grant(
                antenatalRecord.Id,
                patient.Id,
                doctor.Id,
                request.ShareAntenatalRecord,
                request.ShareBirthPlan,
                grantedAtUtc);
            await maternalCareAccessRepository.AddAsync(grant, ct);
        }
        else
        {
            latestGrant.Reactivate(request.ShareAntenatalRecord, request.ShareBirthPlan, grantedAtUtc);
            await maternalCareAccessRepository.UpdateAsync(latestGrant, ct);
            grant = latestGrant;
        }

        return grant.ToDto(doctor.FullName);
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
