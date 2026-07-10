using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.MentalHealth.CrisisProtocol;
using MediatR;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.CreateMoodLog;

public sealed class CreateMoodLogCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IMoodLogRepository moodLogRepository,
    IConsecutiveLowMoodPromptService consecutiveLowMoodPromptService,
    ICrisisProtocolService crisisProtocolService,
    TimeProvider timeProvider)
    : IRequestHandler<CreateMoodLogCommand, MoodLogMutationResultDto>
{
    public async Task<MoodLogMutationResultDto> Handle(CreateMoodLogCommand request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var loggedAtUtc = request.LoggedAtUtc ?? nowUtc;
        var notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        var created = await moodLogRepository.AddAsync(
            new MoodLogCreateModel(
                patient.Id,
                request.Rating,
                notes,
                loggedAtUtc,
                nowUtc),
            ct);

        await consecutiveLowMoodPromptService.TryEmitPromptIfThresholdReachedAsync(patient.Id, ct);

        var crisisProtocol = await crisisProtocolService.TryTriggerAsync(
            patient.Id,
            notes,
            CrisisProtocolInputSource.MoodLog,
            ct);

        return new MoodLogMutationResultDto(created, crisisProtocol);
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
