using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using MediatR;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.ListMoodLogs;

public sealed class ListMoodLogsQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IMoodLogRepository moodLogRepository)
    : IRequestHandler<ListMoodLogsQuery, IReadOnlyList<MoodLogDto>>
{
    public async Task<IReadOnlyList<MoodLogDto>> Handle(ListMoodLogsQuery request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        return await moodLogRepository.ListByPatientIdAsync(
            patient.Id,
            request.FromUtc,
            request.ToUtc,
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
