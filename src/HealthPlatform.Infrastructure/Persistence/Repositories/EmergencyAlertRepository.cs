using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Domain.NextOfKin;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class EmergencyAlertRepository(ApplicationDbContext db) : IEmergencyAlertRepository
{
    public async Task AddAsync(EmergencyAlert alert, CancellationToken ct)
    {
        await db.EmergencyAlerts.AddAsync(alert, ct);
        if (alert.ContactDeliveries.Count > 0)
        {
            await db.EmergencyAlertContactDeliveries.AddRangeAsync(alert.ContactDeliveries, ct);
        }
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
