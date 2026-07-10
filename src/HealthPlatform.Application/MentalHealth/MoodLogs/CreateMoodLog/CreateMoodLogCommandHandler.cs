using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using MediatR;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.CreateMoodLog;

public sealed class CreateMoodLogCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IMoodLogRepository moodLogRepository,
    TimeProvider timeProvider)
    : IRequestHandler<CreateMoodLogCommand, MoodLogDto>
{
    public async Task<MoodLogDto> Handle(CreateMoodLogCommand request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var loggedAtUtc = request.LoggedAtUtc ?? nowUtc;

        return await moodLogRepository.AddAsync(
            new MoodLogCreateModel(
                patient.Id,
                request.Rating,
                string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
                loggedAtUtc,
                nowUtc),
            ct);
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
