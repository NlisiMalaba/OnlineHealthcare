using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using MediatR;

namespace HealthPlatform.Application.Queue.Manage;

public sealed class AdvanceQueueCommandHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    IQueueEntryRepository queueEntryRepository)
    : IRequestHandler<AdvanceQueueCommand, IReadOnlyList<QueueEntryDto>>
{
    public async Task<IReadOnlyList<QueueEntryDto>> Handle(AdvanceQueueCommand request, CancellationToken ct)
    {
        var doctor = await ResolveActingDoctorAsync(ct);
        var activeEntries = await queueEntryRepository.ListActiveByDoctorIdAsync(doctor.Id, ct);
        if (activeEntries.Count == 0)
        {
            return [];
        }

        activeEntries[0].MarkCalled();
        var averageDuration = QueueConsultationDurationResolver.ResolveAverageMinutes(doctor.AvailabilitySlots);
        QueueProjectionUpdater.Recalculate(activeEntries, averageDuration);
        await queueEntryRepository.SaveChangesAsync(ct);
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
