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

    public Task<EmergencyAlert?> GetByIdAsync(Guid alertId, CancellationToken ct) =>
        db.EmergencyAlerts.FirstOrDefaultAsync(alert => alert.Id == alertId, ct);

    public Task<EmergencyAlertContactDelivery?> GetContactDeliveryAsync(
        Guid alertId,
        Guid contactId,
        CancellationToken ct) =>
        db.EmergencyAlertContactDeliveries
            .FirstOrDefaultAsync(
                delivery => delivery.EmergencyAlertId == alertId && delivery.NextOfKinContactId == contactId,
                ct);

    public async Task<IReadOnlyList<EmergencyAlertContactDelivery>> ListContactDeliveriesAsync(
        Guid alertId,
        CancellationToken ct) =>
        await db.EmergencyAlertContactDeliveries
            .Where(delivery => delivery.EmergencyAlertId == alertId)
            .ToListAsync(ct);

    public Task UpdateContactDeliveryAsync(EmergencyAlertContactDelivery delivery, CancellationToken ct)
    {
        db.EmergencyAlertContactDeliveries.Update(delivery);
        return Task.CompletedTask;
    }

    public async Task UpdateOverallStatusAsync(Guid alertId, CancellationToken ct)
    {
        var alert = await db.EmergencyAlerts.FirstOrDefaultAsync(record => record.Id == alertId, ct);
        if (alert is null)
        {
            return;
        }

        var contactDeliveries = await ListContactDeliveriesAsync(alertId, ct);
        alert.RecordContactDeliveries(contactDeliveries);
        db.EmergencyAlerts.Update(alert);
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
