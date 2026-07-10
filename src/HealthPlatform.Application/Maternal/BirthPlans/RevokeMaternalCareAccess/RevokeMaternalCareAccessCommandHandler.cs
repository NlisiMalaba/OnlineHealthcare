using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Maternal.AntenatalRecords;
using MediatR;

namespace HealthPlatform.Application.Maternal.BirthPlans.RevokeMaternalCareAccess;

public sealed class RevokeMaternalCareAccessCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IAntenatalRecordRepository antenatalRecordRepository,
    IMaternalCareAccessRepository maternalCareAccessRepository,
    TimeProvider timeProvider)
    : IRequestHandler<RevokeMaternalCareAccessCommand, MaternalCareAccessGrantDto>
{
    public async Task<MaternalCareAccessGrantDto> Handle(RevokeMaternalCareAccessCommand request, CancellationToken ct)
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
                "You can only revoke access to your own maternal care records.");
        }

        var grant = await maternalCareAccessRepository.GetActiveGrantAsync(
            antenatalRecord.Id,
            request.DoctorId,
            ct)
            ?? throw new NotFoundException(
                BirthPlanErrorCodes.AccessGrantNotFound,
                "Active maternal care access grant was not found.");

        grant.Revoke(timeProvider.GetUtcNow().UtcDateTime);
        await maternalCareAccessRepository.UpdateAsync(grant, ct);

        var doctor = await doctorRepository.GetByIdAsync(request.DoctorId, ct)
            ?? throw new NotFoundException(
                BirthPlanErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");

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
