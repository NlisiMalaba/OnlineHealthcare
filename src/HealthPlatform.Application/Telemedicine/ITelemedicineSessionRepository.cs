using HealthPlatform.Domain.Telemedicine;

namespace HealthPlatform.Application.Telemedicine;

public interface ITelemedicineSessionRepository
{
    Task AddAsync(TelemedicineSession session, CancellationToken ct);

    Task<TelemedicineSession?> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken ct);

    Task UpdateAsync(TelemedicineSession session, CancellationToken ct);
}
