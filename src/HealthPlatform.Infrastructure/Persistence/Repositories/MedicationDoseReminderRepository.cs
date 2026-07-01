using HealthPlatform.Application.Wellness;
using HealthPlatform.Domain.Wellness;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class MedicationDoseReminderRepository(ApplicationDbContext db) : IMedicationDoseReminderRepository
{
    public async Task<IReadOnlyList<DueMedicationDose>> ListDueDosesAsync(
        DateTime nowUtc,
        TimeSpan lookbackWindow,
        int batchSize,
        CancellationToken ct)
    {
        var windowStartUtc = nowUtc - lookbackWindow;
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
        var sentReminders = await db.MedicationDoseReminders
            .AsNoTracking()
            .Where(reminder => scheduleIds.Contains(reminder.ScheduleId))
            .Select(reminder => new { reminder.ScheduleId, reminder.ScheduledAtUtc })
            .ToListAsync(ct);

        var sentReminderKeys = sentReminders
            .Select(reminder => (reminder.ScheduleId, reminder.ScheduledAtUtc))
            .ToHashSet();

        var dueDoses = new List<DueMedicationDose>();
        foreach (var schedule in activeSchedules)
        {
            foreach (var doseTime in schedule.DoseTimes)
            {
                if (doseTime > nowUtc || doseTime <= windowStartUtc)
                {
                    continue;
                }

                if (sentReminderKeys.Contains((schedule.Id, doseTime)))
                {
                    continue;
                }

                dueDoses.Add(new DueMedicationDose(
                    schedule.Id,
                    schedule.PatientId,
                    schedule.MedicationName,
                    doseTime));

                if (dueDoses.Count >= batchSize)
                {
                    return dueDoses;
                }
            }
        }

        return dueDoses;
    }

    public async Task AddSentReminderAsync(MedicationDoseReminder reminder, CancellationToken ct) =>
        await db.MedicationDoseReminders.AddAsync(reminder, ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
