using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Maternal.ChildProfiles;
using HealthPlatform.Application.Maternal.GrowthEntries;
using HealthPlatform.Application.Security;
using MediatR;

namespace HealthPlatform.Application.Maternal.GrowthEntries.ListGrowthEntries;

public sealed class ListGrowthEntriesQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IChildProfileRepository childProfileRepository,
    IGrowthEntryRepository growthEntryRepository)
    : IRequestHandler<ListGrowthEntriesQuery, IReadOnlyList<GrowthEntryDto>>
{
    public async Task<IReadOnlyList<GrowthEntryDto>> Handle(ListGrowthEntriesQuery request, CancellationToken ct)
    {
        var childProfile = await ResolveAccessibleChildProfileAsync(request.ChildProfileId, ct);
        var entries = await growthEntryRepository.ListByChildProfileIdAsync(childProfile.Id, ct);

        return entries
            .Select(entry => entry.ToDto(childProfile.DateOfBirth))
            .ToList();
    }

    private async Task<Domain.Maternal.ChildProfile> ResolveAccessibleChildProfileAsync(
        Guid childProfileId,
        CancellationToken ct)
    {
        var guardian = await ResolveGuardianAsync(ct);
        var childProfile = await childProfileRepository.GetByIdAsync(childProfileId, ct)
            ?? throw new NotFoundException(
                GrowthEntryErrorCodes.ChildProfileNotFound,
                "Child profile was not found.");

        if (childProfile.GuardianId != guardian.Id)
        {
            throw new AccessDeniedException(
                GrowthEntryErrorCodes.AccessDenied,
                "You do not have access to this child profile.");
        }

        return childProfile;
    }

    private async Task<Domain.Identity.Patient> ResolveGuardianAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(
                GrowthEntryErrorCodes.PatientNotFound,
                "Patient profile was not found.");
    }
}
