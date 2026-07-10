using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Maternal.ChildProfiles;
using HealthPlatform.Application.Maternal.GrowthEntries;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.Maternal;
using MediatR;

namespace HealthPlatform.Application.Maternal.GrowthEntries.GetChildGrowthChart;

public sealed class GetChildGrowthChartQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IChildProfileRepository childProfileRepository,
    IGrowthEntryRepository growthEntryRepository)
    : IRequestHandler<GetChildGrowthChartQuery, GrowthChartDto>
{
    public async Task<GrowthChartDto> Handle(GetChildGrowthChartQuery request, CancellationToken ct)
    {
        var childProfile = await ResolveAccessibleChildProfileAsync(request.ChildProfileId, ct);
        var entries = await growthEntryRepository.ListByChildProfileIdAsync(childProfile.Id, ct);
        var maxAgeMonths = ResolveChartMaxAgeMonths(childProfile.DateOfBirth, entries);

        return new GrowthChartDto(
            childProfile.Id,
            childProfile.DateOfBirth,
            ChildGrowthReferencePolicies.BuildHeightReferenceCurves(maxAgeMonths)
                .Select(curve => curve.ToDto())
                .ToList(),
            ChildGrowthReferencePolicies.BuildWeightReferenceCurves(maxAgeMonths)
                .Select(curve => curve.ToDto())
                .ToList(),
            entries
                .Select(entry => entry.ToDto(childProfile.DateOfBirth))
                .ToList());
    }

    private static int ResolveChartMaxAgeMonths(
        DateOnly dateOfBirth,
        IReadOnlyList<GrowthEntry> entries)
    {
        var currentAgeMonths = ChildGrowthReferencePolicies.CalculateAgeMonths(
            dateOfBirth,
            DateTime.UtcNow);

        var maxRecordedAgeMonths = entries.Count == 0
            ? 0
            : entries.Max(entry =>
                ChildGrowthReferencePolicies.CalculateAgeMonths(dateOfBirth, entry.RecordedAtUtc));

        return Math.Clamp(Math.Max(currentAgeMonths, maxRecordedAgeMonths) + 3, 12, 60);
    }

    private async Task<ChildProfile> ResolveAccessibleChildProfileAsync(
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
