using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using MediatR;

namespace HealthPlatform.Application.Maternal.BirthPlans;

public sealed class MaternalCareAccessGuard(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IMaternalCareAccessRepository maternalCareAccessRepository) : IMaternalCareAccessGuard
{
    public async Task EnsureCanReadBirthPlanAsync(
        Guid antenatalRecordId,
        Guid patientId,
        Guid obstetricDoctorId,
        CancellationToken ct)
    {
        if (await IsPatientOwnerAsync(patientId, ct))
        {
            return;
        }

        var doctor = await ResolveDoctorAsync(ct);
        if (doctor.Id == obstetricDoctorId)
        {
            return;
        }

        var grant = await maternalCareAccessRepository.GetActiveGrantAsync(antenatalRecordId, doctor.Id, ct);
        if (grant is { ShareBirthPlan: true })
        {
            return;
        }

        throw new AccessDeniedException(
            BirthPlanErrorCodes.AccessDenied,
            "You do not have access to this birth plan.");
    }

    public async Task EnsureCanReadAntenatalRecordAsync(
        Guid antenatalRecordId,
        Guid patientId,
        Guid obstetricDoctorId,
        CancellationToken ct)
    {
        if (await IsPatientOwnerAsync(patientId, ct))
        {
            return;
        }

        var doctor = await ResolveDoctorAsync(ct);
        if (doctor.Id == obstetricDoctorId)
        {
            return;
        }

        var grant = await maternalCareAccessRepository.GetActiveGrantAsync(antenatalRecordId, doctor.Id, ct);
        if (grant is { ShareAntenatalRecord: true })
        {
            return;
        }

        throw new AccessDeniedException(
            BirthPlanErrorCodes.AccessDenied,
            "You do not have access to this antenatal record.");
    }

    private async Task<bool> IsPatientOwnerAsync(Guid patientId, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        if (!userId.HasValue)
        {
            return false;
        }

        var patient = await patientRepository.GetByUserIdAsync(userId.Value, ct);
        return patient?.Id == patientId;
    }

    private async Task<Domain.Identity.Doctor> ResolveDoctorAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct)
            ?? throw new AccessDeniedException(
                BirthPlanErrorCodes.AccessDenied,
                "Doctor profile was not found.");
    }
}
