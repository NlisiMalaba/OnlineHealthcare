using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Maternal;
using HealthPlatform.Application.Maternal.ChildProfiles;
using HealthPlatform.Application.Vaccinations;
using MediatR;

namespace HealthPlatform.Application.Maternal.ChildProfiles.CreateChildProfile;

public sealed class CreateChildProfileCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IHealthRecordRepository healthRecordRepository,
    IChildProfileRepository childProfileRepository,
    IVaccinationScheduleInitializer vaccinationScheduleInitializer,
    TimeProvider timeProvider)
    : IRequestHandler<CreateChildProfileCommand, ChildProfileDto>
{
    public async Task<ChildProfileDto> Handle(CreateChildProfileCommand request, CancellationToken ct)
    {
        var guardian = await ResolveGuardianAsync(ct);
        var createdAtUtc = timeProvider.GetUtcNow().UtcDateTime;

        var healthRecord = HealthRecord.CreateForChildProfile(guardian.Id);
        await healthRecordRepository.AddAsync(healthRecord, ct);

        var childProfile = ChildProfile.Create(
            guardian.Id,
            request.FullName,
            request.DateOfBirth,
            request.BloodType,
            request.KnownAllergies,
            healthRecord.Id,
            createdAtUtc);

        await childProfileRepository.AddAsync(childProfile, ct);
        healthRecord.AssignChildProfile(childProfile.Id);
        await healthRecordRepository.SaveChangesAsync(ct);

        await vaccinationScheduleInitializer.InitializeChildScheduleAsync(
            childProfile.Id,
            childProfile.DateOfBirth,
            createdAtUtc,
            ct);

        return childProfile.ToDto();
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
