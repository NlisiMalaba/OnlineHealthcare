using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Prescriptions;
using HealthPlatform.Domain.Identity;
using MediatR;

namespace HealthPlatform.Application.Wellness.Summaries;

public sealed class GetDoctorPatientAdherenceSummaryQueryHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    IPrescriptionRepository prescriptionRepository,
    IMedicationScheduleRepository medicationScheduleRepository,
    IAdherenceEventRepository adherenceEventRepository,
    TimeProvider timeProvider)
    : IRequestHandler<GetDoctorPatientAdherenceSummaryQuery, AdherenceSummaryDto>
{
    public async Task<AdherenceSummaryDto> Handle(GetDoctorPatientAdherenceSummaryQuery request, CancellationToken ct)
    {
        var doctor = await ResolveDoctorAsync(ct);
        var (fromUtc, toUtc) = AdherenceSummaryWindow.Resolve(request.Period, timeProvider.GetUtcNow().UtcDateTime);

        var doctorPrescriptionIds = (await prescriptionRepository.ListByDoctorIdAsync(doctor.Id, ct))
            .Where(prescription => prescription.PatientId == request.PatientId)
            .Select(prescription => prescription.Id)
            .ToList();

        if (doctorPrescriptionIds.Count == 0)
        {
            throw new AccessDeniedException(
                WellnessErrorCodes.AdherenceSummaryAccessDenied,
                "The doctor has no prescriptions for this patient.");
        }

        var schedules = await medicationScheduleRepository.ListByPrescriptionIdsAsync(doctorPrescriptionIds, ct);
        var scheduleIds = schedules.Select(schedule => schedule.Id).ToList();
        var events = await adherenceEventRepository.ListByScheduleIdsInRangeAsync(scheduleIds, fromUtc, toUtc, ct);

        return AdherenceSummaryBuilder.Build(request.PatientId, request.Period, fromUtc, toUtc, schedules, events);
    }

    private async Task<Doctor> ResolveDoctorAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct)
            ?? throw new NotFoundException(
                WellnessErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");
    }
}
