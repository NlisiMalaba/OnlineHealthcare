using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using MediatR;

namespace HealthPlatform.Application.Queue.Manage;

public sealed class RecalculateQueueOnDelayCommandHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    IPatientRepository patientRepository,
    IQueueEntryRepository queueEntryRepository,
    IQueueDelayNotifier queueDelayNotifier)
    : IRequestHandler<RecalculateQueueOnDelayCommand, IReadOnlyList<QueueEntryDto>>
{
    public async Task<IReadOnlyList<QueueEntryDto>> Handle(RecalculateQueueOnDelayCommand request, CancellationToken ct)
    {
        var doctor = await ResolveActingDoctorAsync(ct);
        if (request.DelayMinutes <= QueuePolicies.DelayRecalculationThresholdMinutes)
        {
            throw new DomainException(
                QueueErrorCodes.QueueDelayBelowThreshold,
                $"Delay must be greater than {QueuePolicies.DelayRecalculationThresholdMinutes} minutes.");
        }

        var activeEntries = await queueEntryRepository.ListActiveByDoctorIdAsync(doctor.Id, ct);
        if (activeEntries.Count == 0)
        {
            return [];
        }

        var averageDuration = QueueConsultationDurationResolver.ResolveAverageMinutes(doctor.AvailabilitySlots);
        QueueProjectionUpdater.Recalculate(activeEntries, averageDuration, request.DelayMinutes);
        await queueEntryRepository.SaveChangesAsync(ct);

        foreach (var entry in activeEntries)
        {
            var patient = await patientRepository.GetByIdAsync(entry.PatientId, ct);
            if (patient is null)
            {
                continue;
            }

            await queueDelayNotifier.NotifyQueueDelayRecalculatedAsync(
                patient.UserId,
                entry.Id,
                entry.AppointmentId,
                entry.EstimatedWaitMinutes,
                request.DelayMinutes,
                ct);
        }

        return activeEntries.Select(entry => entry.ToDto()).ToList();
    }

    private async Task<Domain.Identity.Doctor> ResolveActingDoctorAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct)
            ?? throw new AccessDeniedException(
                QueueErrorCodes.QueueActionAccessDenied,
                "Only clinic doctors can manage queue entries.");
    }
}
