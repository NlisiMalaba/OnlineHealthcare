using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using MediatR;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.DeleteMoodLog;

public sealed class DeleteMoodLogCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IMoodLogRepository moodLogRepository,
    TimeProvider timeProvider)
    : IRequestHandler<DeleteMoodLogCommand>
{
    public async Task Handle(DeleteMoodLogCommand request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        _ = await moodLogRepository.GetByIdForPatientAsync(request.MoodLogId, patient.Id, ct)
            ?? throw new NotFoundException(
                MoodLogErrorCodes.MoodLogNotFound,
                "Mood log was not found.");

        var deleted = await moodLogRepository.DeleteAsync(
            request.MoodLogId,
            timeProvider.GetUtcNow().UtcDateTime,
            ct);

        if (!deleted)
        {
            throw new NotFoundException(
                MoodLogErrorCodes.MoodLogNotFound,
                "Mood log was not found.");
        }
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
