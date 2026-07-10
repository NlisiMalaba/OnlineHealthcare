using HealthPlatform.Domain.Queue;

namespace HealthPlatform.Application.Queue;

public interface IQueueEntryRepository
{
    Task AddAsync(QueueEntry entry, CancellationToken ct);

    Task UpdateAsync(QueueEntry entry, CancellationToken ct);

    Task DeleteAsync(QueueEntry entry, CancellationToken ct);

    Task<bool> ExistsActiveForAppointmentAsync(Guid appointmentId, CancellationToken ct);

    Task<int> CountActiveByDoctorIdAsync(Guid doctorId, CancellationToken ct);

    Task<IReadOnlyList<QueueEntry>> ListActiveAsync(CancellationToken ct);

    Task<IReadOnlyList<QueueEntry>> ListActiveByDoctorIdAsync(Guid doctorId, CancellationToken ct);

    Task<QueueEntry?> GetByIdAsync(Guid queueEntryId, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
