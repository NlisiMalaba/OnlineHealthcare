using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Maternal.ChildProfiles;
using HealthPlatform.Application.Security;
using HealthPlatform.Application.Vaccinations;
using MediatR;

namespace HealthPlatform.Application.Vaccinations.ListChildVaccinationSchedule;

public sealed class ListChildVaccinationScheduleQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IChildProfileRepository childProfileRepository,
    IVaccinationScheduleRepository scheduleRepository,
    IVaccinationScheduleInitializer scheduleInitializer,
    TimeProvider timeProvider)
    : IRequestHandler<ListChildVaccinationScheduleQuery, IReadOnlyList<VaccinationScheduleEntryDto>>
{
    public async Task<IReadOnlyList<VaccinationScheduleEntryDto>> Handle(
        ListChildVaccinationScheduleQuery request,
        CancellationToken ct)
    {
        var childProfile = await ResolveAccessibleChildProfileAsync(request.ChildProfileId, ct);
        var now = timeProvider.GetUtcNow().UtcDateTime;

        if (!await scheduleRepository.HasScheduleForChildAsync(childProfile.Id, ct))
        {
            await scheduleInitializer.InitializeChildScheduleAsync(
                childProfile.Id,
                childProfile.DateOfBirth,
                now,
                ct);
        }

        var entries = await scheduleRepository.ListByChildProfileIdAsync(childProfile.Id, ct);
        return entries.Select(entry => entry.ToDto()).ToList();
    }

    private async Task<Domain.Maternal.ChildProfile> ResolveAccessibleChildProfileAsync(
        Guid childProfileId,
        CancellationToken ct)
    {
        var guardian = await ResolveGuardianAsync(ct);
        var childProfile = await childProfileRepository.GetByIdAsync(childProfileId, ct)
            ?? throw new NotFoundException(
                VaccinationErrorCodes.ChildProfileNotFound,
                "Child profile was not found.");

        if (childProfile.GuardianId != guardian.Id)
        {
            throw new AccessDeniedException(
                VaccinationErrorCodes.AccessDenied,
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
                VaccinationErrorCodes.PatientNotFound,
                "Patient profile was not found.");
    }
}
