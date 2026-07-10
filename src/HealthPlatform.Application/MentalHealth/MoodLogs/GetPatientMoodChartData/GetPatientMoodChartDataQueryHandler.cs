using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using MediatR;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.GetPatientMoodChartData;

public sealed class GetPatientMoodChartDataQueryHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    IMoodLogRepository moodLogRepository,
    IMoodChartSharingConsentRepository moodChartSharingConsentRepository,
    TimeProvider timeProvider)
    : IRequestHandler<GetPatientMoodChartDataQuery, MoodChartDataDto>
{
    public async Task<MoodChartDataDto> Handle(GetPatientMoodChartDataQuery request, CancellationToken ct)
    {
        var therapist = await ResolveTherapistAsync(ct);

        if (!TherapistPolicies.IsLicensedTherapist(therapist.Specialty))
        {
            throw new DomainException(
                MoodLogErrorCodes.TherapistRequired,
                "Only a licensed therapist can view patient mood chart data.");
        }

        var consent = await moodChartSharingConsentRepository.GetActiveConsentAsync(
            request.PatientId,
            therapist.Id,
            ct);

        if (consent is null)
        {
            throw new AccessDeniedException(
                MoodLogErrorCodes.MoodChartConsentRequired,
                "Patient has not granted mood chart sharing consent.");
        }

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var toUtc = request.ToUtc ?? nowUtc;
        var fromUtc = request.FromUtc ?? toUtc.Subtract(MoodLogPolicies.DefaultChartWindow);
        var logs = await moodLogRepository.ListByPatientIdAsync(request.PatientId, fromUtc, toUtc, ct);
        return MoodLogMappings.ToChartDto(request.PatientId, fromUtc, toUtc, logs);
    }

    private async Task<Domain.Identity.Doctor> ResolveTherapistAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct)
            ?? throw new NotFoundException(
                MoodLogErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");
    }
}
