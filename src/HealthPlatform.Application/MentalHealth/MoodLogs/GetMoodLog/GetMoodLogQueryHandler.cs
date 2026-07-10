using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using MediatR;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.GetMoodLog;

public sealed class GetMoodLogQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IMoodLogRepository moodLogRepository)
    : IRequestHandler<GetMoodLogQuery, MoodLogDto>
{
    public async Task<MoodLogDto> Handle(GetMoodLogQuery request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        return await moodLogRepository.GetByIdForPatientAsync(request.MoodLogId, patient.Id, ct)
            ?? throw new NotFoundException(
                MoodLogErrorCodes.MoodLogNotFound,
                "Mood log was not found.");
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
