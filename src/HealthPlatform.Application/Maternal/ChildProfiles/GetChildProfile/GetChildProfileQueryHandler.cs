using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using MediatR;

namespace HealthPlatform.Application.Maternal.ChildProfiles.GetChildProfile;

public sealed class GetChildProfileQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IChildProfileRepository childProfileRepository)
    : IRequestHandler<GetChildProfileQuery, ChildProfileDto>
{
    public async Task<ChildProfileDto> Handle(GetChildProfileQuery request, CancellationToken ct)
    {
        var guardian = await ResolveGuardianAsync(ct);
        var profile = await childProfileRepository.GetByIdAsync(request.ChildProfileId, ct)
            ?? throw new NotFoundException(
                ChildProfileErrorCodes.ChildProfileNotFound,
                "Child profile was not found.");

        if (profile.GuardianId != guardian.Id)
        {
            throw new AccessDeniedException(
                ChildProfileErrorCodes.AccessDenied,
                "You do not have access to this child profile.");
        }

        return profile.ToDto();
    }

    private async Task<Domain.Identity.Patient> ResolveGuardianAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(
                ChildProfileErrorCodes.PatientNotFound,
                "Patient profile was not found.");
    }
}
