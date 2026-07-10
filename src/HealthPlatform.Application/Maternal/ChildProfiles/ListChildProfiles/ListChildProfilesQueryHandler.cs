using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using MediatR;

namespace HealthPlatform.Application.Maternal.ChildProfiles.ListChildProfiles;

public sealed class ListChildProfilesQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IChildProfileRepository childProfileRepository)
    : IRequestHandler<ListChildProfilesQuery, IReadOnlyList<ChildProfileDto>>
{
    public async Task<IReadOnlyList<ChildProfileDto>> Handle(ListChildProfilesQuery request, CancellationToken ct)
    {
        var guardian = await ResolveGuardianAsync(ct);
        var profiles = await childProfileRepository.ListByGuardianIdAsync(guardian.Id, ct);
        return profiles.Select(profile => profile.ToDto()).ToList();
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
