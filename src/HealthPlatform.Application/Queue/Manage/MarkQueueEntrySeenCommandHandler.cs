using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.Queue;
using MediatR;

namespace HealthPlatform.Application.Queue.Manage;

public sealed class MarkQueueEntrySeenCommandHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    IQueueEntryRepository queueEntryRepository,
    TimeProvider timeProvider)
    : IRequestHandler<MarkQueueEntrySeenCommand, QueueEntryDto>
{
    public async Task<QueueEntryDto> Handle(MarkQueueEntrySeenCommand request, CancellationToken ct)
    {
        var doctor = await ResolveActingDoctorAsync(ct);
        var entry = await queueEntryRepository.GetByIdAsync(request.QueueEntryId, ct)
            ?? throw new NotFoundException(
                QueueErrorCodes.QueueEntryNotFound,
                "Queue entry was not found.");

        EnsureDoctorOwnsEntry(doctor.Id, entry);

        if (!entry.MarkSeen(timeProvider.GetUtcNow().UtcDateTime))
        {
            throw new DomainException(
                QueueErrorCodes.QueueEntryAlreadyClosed,
                "Queue entry is already marked as seen or absent.");
        }

        await queueEntryRepository.SaveChangesAsync(ct);
        var activeEntries = await queueEntryRepository.ListActiveByDoctorIdAsync(doctor.Id, ct);
        var averageDuration = QueueConsultationDurationResolver.ResolveAverageMinutes(doctor.AvailabilitySlots);
        QueueProjectionUpdater.Recalculate(activeEntries, averageDuration);
        await queueEntryRepository.SaveChangesAsync(ct);
        return entry.ToDto();
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
