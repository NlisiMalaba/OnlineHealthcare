using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.MentalHealth.CrisisProtocol;
using MediatR;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.UpdateMoodLog;

public sealed class UpdateMoodLogCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IMoodLogRepository moodLogRepository,
    IConsecutiveLowMoodPromptService consecutiveLowMoodPromptService,
    ICrisisProtocolService crisisProtocolService,
    TimeProvider timeProvider)
    : IRequestHandler<UpdateMoodLogCommand, MoodLogMutationResultDto>
{
    public async Task<MoodLogMutationResultDto> Handle(UpdateMoodLogCommand request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var existing = await moodLogRepository.GetByIdForPatientAsync(request.MoodLogId, patient.Id, ct)
            ?? throw new NotFoundException(
                MoodLogErrorCodes.MoodLogNotFound,
                "Mood log was not found.");

        var updatedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        var updated = await moodLogRepository.UpdateAsync(
            new MoodLogUpdateModel(
                request.MoodLogId,
                request.Rating,
                notes,
                request.LoggedAtUtc ?? existing.LoggedAtUtc,
                updatedAtUtc),
            ct);

        if (!updated)
        {
            throw new NotFoundException(
                MoodLogErrorCodes.MoodLogNotFound,
                "Mood log was not found.");
        }

        await consecutiveLowMoodPromptService.TryEmitPromptIfThresholdReachedAsync(patient.Id, ct);

        var moodLog = (await moodLogRepository.GetByIdForPatientAsync(request.MoodLogId, patient.Id, ct))!;
        var crisisProtocol = await crisisProtocolService.TryTriggerAsync(
            patient.Id,
            notes,
            CrisisProtocolInputSource.MoodLog,
            ct);

        return new MoodLogMutationResultDto(moodLog, crisisProtocol);
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
