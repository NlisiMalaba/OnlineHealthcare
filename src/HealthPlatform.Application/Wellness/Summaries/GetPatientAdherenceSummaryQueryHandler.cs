using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.Identity;
using MediatR;

namespace HealthPlatform.Application.Wellness.Summaries;

public sealed class GetPatientAdherenceSummaryQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IMedicationScheduleRepository medicationScheduleRepository,
    IAdherenceEventRepository adherenceEventRepository,
    TimeProvider timeProvider)
    : IRequestHandler<GetPatientAdherenceSummaryQuery, AdherenceSummaryDto>
{
    public async Task<AdherenceSummaryDto> Handle(GetPatientAdherenceSummaryQuery request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var (fromUtc, toUtc) = AdherenceSummaryWindow.Resolve(request.Period, timeProvider.GetUtcNow().UtcDateTime);

        var schedules = await medicationScheduleRepository.ListByPatientIdAsync(patient.Id, ct);
        var scheduleIds = schedules.Select(schedule => schedule.Id).ToList();
        var events = await adherenceEventRepository.ListByScheduleIdsInRangeAsync(scheduleIds, fromUtc, toUtc, ct);

        return AdherenceSummaryBuilder.Build(patient.Id, request.Period, fromUtc, toUtc, schedules, events);
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
