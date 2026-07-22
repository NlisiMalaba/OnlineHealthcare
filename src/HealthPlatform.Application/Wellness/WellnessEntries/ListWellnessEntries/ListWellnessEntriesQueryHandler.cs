using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.Identity;
using MediatR;

namespace HealthPlatform.Application.Wellness.WellnessEntries.ListWellnessEntries;

public sealed class ListWellnessEntriesQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IWellnessEntryRepository wellnessEntryRepository)
    : IRequestHandler<ListWellnessEntriesQuery, IReadOnlyList<WellnessEntryDto>>
{
    public async Task<IReadOnlyList<WellnessEntryDto>> Handle(ListWellnessEntriesQuery request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var entries = await wellnessEntryRepository.ListByPatientIdAsync(
            patient.Id,
            request.MetricType,
            request.FromUtc,
            request.ToUtc,
            ct);

        return entries.Select(entry => entry.ToDto()).ToList();
    }

    private async Task<Patient> ResolvePatientAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(
                WellnessErrorCodes.PatientNotFound,
                "Patient profile was not found.");
    }
}
