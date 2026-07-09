using HealthPlatform.Domain.Queue;

namespace HealthPlatform.Application.Queue;

public interface IQueueEntryRepository
{
    Task AddAsync(QueueEntry entry, CancellationToken ct);

    Task<bool> ExistsActiveForAppointmentAsync(Guid appointmentId, CancellationToken ct);

    Task<int> CountActiveByDoctorIdAsync(Guid doctorId, CancellationToken ct);
}
