using HealthPlatform.Application.Wellness;
using HealthPlatform.Domain.Wellness;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class AdherenceEventRepository(ApplicationDbContext db) : IAdherenceEventRepository
{
    public Task<AdherenceEvent?> GetByScheduleAndScheduledAtAsync(
        Guid scheduleId,
        DateTime scheduledAtUtc,
        CancellationToken ct) =>
        db.AdherenceEvents.SingleOrDefaultAsync(
            adherenceEvent => adherenceEvent.ScheduleId == scheduleId
                && adherenceEvent.ScheduledAtUtc == scheduledAtUtc,
            ct);

    public async Task<IReadOnlyList<OverdueMedicationDose>> ListOverdueUnconfirmedDosesAsync(
        DateTime nowUtc,
        int batchSize,
        CancellationToken ct)
    {
        var missedThresholdUtc = nowUtc - WellnessPolicies.MissedDoseGracePeriod;
        var activeSchedules = await db.MedicationSchedules
            .AsNoTracking()
            .Where(schedule => schedule.Status == MedicationScheduleStatus.Active)
            .Take(batchSize)
            .ToListAsync(ct);

        if (activeSchedules.Count == 0)
        {
            return [];
        }

        var scheduleIds = activeSchedules.Select(schedule => schedule.Id).ToList();
        var recordedEvents = await db.AdherenceEvents
            .AsNoTracking()
            .Where(adherenceEvent => scheduleIds.Contains(adherenceEvent.ScheduleId))
            .Select(adherenceEvent => new { adherenceEvent.ScheduleId, adherenceEvent.ScheduledAtUtc })
            .ToListAsync(ct);

        var recordedKeys = recordedEvents
            .Select(adherenceEvent => (adherenceEvent.ScheduleId, adherenceEvent.ScheduledAtUtc))
            .ToHashSet();

        var overdueDoses = new List<OverdueMedicationDose>();
        foreach (var schedule in activeSchedules)
        {
            foreach (var doseTime in schedule.DoseTimes)
            {
                if (doseTime > missedThresholdUtc)
                {
                    continue;
                }

                if (recordedKeys.Contains((schedule.Id, doseTime)))
                {
                    continue;
                }

                overdueDoses.Add(new OverdueMedicationDose(
                    schedule.Id,
                    schedule.PatientId,
                    doseTime));

                if (overdueDoses.Count >= batchSize)
                {
                    return overdueDoses;
                }
            }
        }

        return overdueDoses;
    }

    public async Task AddAsync(AdherenceEvent adherenceEvent, CancellationToken ct) =>
        await db.AdherenceEvents.AddAsync(adherenceEvent, ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
