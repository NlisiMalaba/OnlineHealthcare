using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Wellness;
using MediatR;

namespace HealthPlatform.Application.Wellness.ConfirmMedicationDose;

public sealed class ConfirmMedicationDoseCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IMedicationScheduleRepository medicationScheduleRepository,
    IAdherenceEventRepository adherenceEventRepository,
    TimeProvider timeProvider)
    : IRequestHandler<ConfirmMedicationDoseCommand, AdherenceEventDto>
{
    public async Task<AdherenceEventDto> Handle(ConfirmMedicationDoseCommand request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var schedule = await medicationScheduleRepository.GetActiveByIdForPatientAsync(
            request.ScheduleId,
            patient.Id,
            ct)
            ?? throw new NotFoundException(
                WellnessErrorCodes.ScheduleNotFound,
                "Medication schedule was not found.");

        if (!schedule.DoseTimes.Contains(request.ScheduledAtUtc))
        {
            throw new DomainException(
                WellnessErrorCodes.DoseNotOnSchedule,
                "The specified dose time is not part of this medication schedule.");
        }

        var existingEvent = await adherenceEventRepository.GetByScheduleAndScheduledAtAsync(
            request.ScheduleId,
            request.ScheduledAtUtc,
            ct);
        if (existingEvent is not null)
        {
            throw new ConflictException(
                WellnessErrorCodes.DoseAlreadyRecorded,
                "An adherence event has already been recorded for this dose.");
        }

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        if (nowUtc < request.ScheduledAtUtc)
        {
            throw new DomainException(
                WellnessErrorCodes.CannotConfirmFutureDose,
                "Future doses cannot be confirmed.");
        }

        if (!WellnessPolicies.CanConfirmDose(request.ScheduledAtUtc, nowUtc))
        {
            throw new DomainException(
                WellnessErrorCodes.DoseConfirmationWindowExpired,
                "The confirmation window for this dose has expired.");
        }

        var adherenceEvent = AdherenceEvent.RecordTaken(
            schedule.Id,
            patient.Id,
            request.ScheduledAtUtc,
            nowUtc);

        await adherenceEventRepository.AddAsync(adherenceEvent, ct);
        await adherenceEventRepository.SaveChangesAsync(ct);
        return adherenceEvent.ToDto();
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
