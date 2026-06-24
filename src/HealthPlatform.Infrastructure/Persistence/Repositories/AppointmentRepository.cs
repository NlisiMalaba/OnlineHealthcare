using HealthPlatform.Application.Appointments;
using HealthPlatform.Domain.Appointments;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class AppointmentRepository(ApplicationDbContext db) : IAppointmentRepository
{
    public async Task AddAsync(Appointment appointment, CancellationToken ct)
    {
        await db.Appointments.AddAsync(appointment, ct);
        await db.SaveChangesAsync(ct);
    }
}
