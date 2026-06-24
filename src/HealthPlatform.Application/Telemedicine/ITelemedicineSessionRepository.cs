using HealthPlatform.Domain.Telemedicine;

namespace HealthPlatform.Application.Telemedicine;

public interface ITelemedicineSessionRepository
{
    Task AddAsync(TelemedicineSession session, CancellationToken ct);

    Task<TelemedicineSession?> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken ct);

    Task<IReadOnlyList<TelemedicineSession>> ListActiveSessionsAsync(CancellationToken ct);

    Task<IReadOnlyList<TelemedicineSession>> ListSessionsWithPendingReconnectionGraceAsync(
        DateTime asOfUtc,
        TimeSpan gracePeriod,
        CancellationToken ct);

    Task UpdateAsync(TelemedicineSession session, CancellationToken ct);
}
