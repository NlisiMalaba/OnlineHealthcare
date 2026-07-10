using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.MentalHealth;
using MediatR;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.GrantMoodChartSharingConsent;

public sealed class GrantMoodChartSharingConsentCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IMoodChartSharingConsentRepository moodChartSharingConsentRepository,
    TimeProvider timeProvider)
    : IRequestHandler<GrantMoodChartSharingConsentCommand, MoodChartSharingConsentDto>
{
    public async Task<MoodChartSharingConsentDto> Handle(
        GrantMoodChartSharingConsentCommand request,
        CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var therapist = await doctorRepository.GetByIdAsync(request.TherapistId, ct)
            ?? throw new NotFoundException(
                MoodLogErrorCodes.DoctorNotFound,
                "Therapist profile was not found.");

        if (!TherapistPolicies.IsLicensedTherapist(therapist.Specialty))
        {
            throw new DomainException(
                MoodLogErrorCodes.TherapistRequired,
                "Mood chart sharing can only be granted to a licensed therapist.");
        }

        var grantedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var existing = await moodChartSharingConsentRepository.GetLatestConsentAsync(
            patient.Id,
            therapist.Id,
            ct);

        if (existing is { IsActive: true })
        {
            return existing.ToDto();
        }

        if (existing is not null)
        {
            existing.Reactivate(grantedAtUtc);
            await moodChartSharingConsentRepository.UpdateAsync(existing, ct);
            return existing.ToDto();
        }

        var consent = MoodChartSharingConsent.Grant(patient.Id, therapist.Id, grantedAtUtc);
        await moodChartSharingConsentRepository.AddAsync(consent, ct);
        return consent.ToDto();
    }

    private async Task<Domain.Identity.Patient> ResolvePatientAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(
                MoodLogErrorCodes.PatientNotFound,
                "Patient profile was not found.");
    }
}
