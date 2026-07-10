using HealthPlatform.Domain.MentalHealth;

namespace HealthPlatform.Application.MentalHealth;

public interface ITherapySessionRepository
{
    Task AddAsync(TherapySession session, CancellationToken ct);

    Task<TherapySession?> GetByIdAsync(Guid therapySessionId, CancellationToken ct);

    Task<TherapySession?> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken ct);

    Task UpdateAsync(TherapySession session, CancellationToken ct);
}
