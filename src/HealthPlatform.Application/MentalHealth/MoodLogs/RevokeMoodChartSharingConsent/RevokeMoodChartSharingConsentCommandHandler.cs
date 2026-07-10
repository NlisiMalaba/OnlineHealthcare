using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using MediatR;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.RevokeMoodChartSharingConsent;

public sealed class RevokeMoodChartSharingConsentCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IMoodChartSharingConsentRepository moodChartSharingConsentRepository,
    TimeProvider timeProvider)
    : IRequestHandler<RevokeMoodChartSharingConsentCommand, MoodChartSharingConsentDto>
{
    public async Task<MoodChartSharingConsentDto> Handle(
        RevokeMoodChartSharingConsentCommand request,
        CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var consent = await moodChartSharingConsentRepository.GetActiveConsentAsync(
            patient.Id,
            request.TherapistId,
            ct)
            ?? throw new NotFoundException(
                MoodLogErrorCodes.MoodChartConsentNotFound,
                "Active mood chart sharing consent was not found.");

        consent.Revoke(timeProvider.GetUtcNow().UtcDateTime);
        await moodChartSharingConsentRepository.UpdateAsync(consent, ct);
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
