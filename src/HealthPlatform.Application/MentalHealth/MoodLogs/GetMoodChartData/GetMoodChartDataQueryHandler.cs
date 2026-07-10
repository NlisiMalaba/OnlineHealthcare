using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using MediatR;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.GetMoodChartData;

public sealed class GetMoodChartDataQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IMoodLogRepository moodLogRepository,
    TimeProvider timeProvider)
    : IRequestHandler<GetMoodChartDataQuery, MoodChartDataDto>
{
    public async Task<MoodChartDataDto> Handle(GetMoodChartDataQuery request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var toUtc = request.ToUtc ?? nowUtc;
        var fromUtc = request.FromUtc ?? toUtc.Subtract(MoodLogPolicies.DefaultChartWindow);

        var logs = await moodLogRepository.ListByPatientIdAsync(patient.Id, fromUtc, toUtc, ct);
        return MoodLogMappings.ToChartDto(patient.Id, fromUtc, toUtc, logs);
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
