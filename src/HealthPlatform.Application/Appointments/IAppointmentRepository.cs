using HealthPlatform.Domain.Appointments;

namespace HealthPlatform.Application.Appointments;

public interface IAppointmentRepository
{
    Task AddAsync(Appointment appointment, CancellationToken ct);
}
