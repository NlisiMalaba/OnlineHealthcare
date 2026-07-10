using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Maternal.ChildProfiles;
using HealthPlatform.Application.Security;
using HealthPlatform.Application.Vaccinations;
using MediatR;

namespace HealthPlatform.Application.Vaccinations.ListChildVaccinationRecords;

public sealed class ListChildVaccinationRecordsQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IChildProfileRepository childProfileRepository,
    IVaccinationRecordRepository recordRepository)
    : IRequestHandler<ListChildVaccinationRecordsQuery, IReadOnlyList<VaccinationRecordDto>>
{
    public async Task<IReadOnlyList<VaccinationRecordDto>> Handle(
        ListChildVaccinationRecordsQuery request,
        CancellationToken ct)
    {
        await EnsureGuardianAccessAsync(request.ChildProfileId, ct);
        var records = await recordRepository.ListByChildProfileIdAsync(request.ChildProfileId, ct);
        return records.Select(record => record.ToDto()).ToList();
    }

    private async Task EnsureGuardianAccessAsync(Guid childProfileId, CancellationToken ct)
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
