using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.Queue;
using MediatR;

namespace HealthPlatform.Application.Queue.Manage;

public sealed class MarkQueueEntryAbsentCommandHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    IPatientRepository patientRepository,
    IQueueEntryRepository queueEntryRepository,
    IQueueStatusNotifier queueStatusNotifier)
    : IRequestHandler<MarkQueueEntryAbsentCommand>
{
    public async Task Handle(MarkQueueEntryAbsentCommand request, CancellationToken ct)
    {
        var doctor = await ResolveActingDoctorAsync(ct);
        var entry = await queueEntryRepository.GetByIdAsync(request.QueueEntryId, ct)
            ?? throw new NotFoundException(
                QueueErrorCodes.QueueEntryNotFound,
                "Queue entry was not found.");

        EnsureDoctorOwnsEntry(doctor.Id, entry);
        if (entry.ArrivalStatus is QueueArrivalStatus.Seen or QueueArrivalStatus.Absent)
        {
            throw new DomainException(
                QueueErrorCodes.QueueEntryAlreadyClosed,
                "Queue entry is already marked as seen or absent.");
        }

        var patient = await patientRepository.GetByIdAsync(entry.PatientId, ct)
            ?? throw new NotFoundException(
                QueueErrorCodes.PatientNotFound,
                "Patient profile was not found.");

        await queueEntryRepository.DeleteAsync(entry, ct);
        await queueStatusNotifier.NotifyMarkedAbsentAsync(
            patient.UserId,
            entry.Id,
            entry.AppointmentId,
            ct);

        var activeEntries = await queueEntryRepository.ListActiveByDoctorIdAsync(doctor.Id, ct);
        var averageDuration = QueueConsultationDurationResolver.ResolveAverageMinutes(doctor.AvailabilitySlots);
        QueueProjectionUpdater.Recalculate(activeEntries, averageDuration);
        await queueEntryRepository.SaveChangesAsync(ct);
    }

    private static void EnsureDoctorOwnsEntry(Guid doctorId, QueueEntry entry)
    {
        if (entry.DoctorId != doctorId)
        {
            throw new AccessDeniedException(
                QueueErrorCodes.QueueActionAccessDenied,
                "You can only manage your own clinic queue entries.");
        }
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
