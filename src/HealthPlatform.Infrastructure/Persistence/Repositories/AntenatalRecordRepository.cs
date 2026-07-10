using HealthPlatform.Application.Maternal.AntenatalRecords;
using HealthPlatform.Domain.Maternal;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class AntenatalRecordRepository(ApplicationDbContext db) : IAntenatalRecordRepository
{
    public async Task AddAsync(AntenatalRecord record, CancellationToken ct)
    {
        await db.AntenatalRecords.AddAsync(record, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task<AntenatalRecord?> GetByIdAsync(Guid antenatalRecordId, CancellationToken ct) =>
        db.AntenatalRecords.SingleOrDefaultAsync(record => record.Id == antenatalRecordId, ct);

    public Task<AntenatalRecord?> GetActiveByPatientIdAsync(Guid patientId, CancellationToken ct) =>
        db.AntenatalRecords.SingleOrDefaultAsync(
            record => record.PatientId == patientId && record.Status == AntenatalRecordStatus.Active,
            ct);

    public Task UpdateAsync(AntenatalRecord record, CancellationToken ct) =>
        db.SaveChangesAsync(ct);

    public async Task AddScheduleEntriesAsync(
        IReadOnlyCollection<AntenatalCheckupScheduleEntry> entries,
        CancellationToken ct)
    {
        await db.AntenatalCheckupScheduleEntries.AddRangeAsync(entries, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<AntenatalCheckupScheduleEntry>> ListScheduleEntriesByRecordIdAsync(
        Guid antenatalRecordId,
        CancellationToken ct) =>
        await db.AntenatalCheckupScheduleEntries
            .Where(entry => entry.AntenatalRecordId == antenatalRecordId)
            .OrderBy(entry => entry.RecommendedDate)
            .ToListAsync(ct);

    public Task<bool> HasScheduleEntriesAsync(Guid antenatalRecordId, CancellationToken ct) =>
        db.AntenatalCheckupScheduleEntries.AnyAsync(entry => entry.AntenatalRecordId == antenatalRecordId, ct);

    public async Task<IReadOnlyList<AntenatalRecord>> ListActiveDueForReminderAsync(
        DateTime asOfUtc,
        CancellationToken ct) =>
        await db.AntenatalRecords
            .Where(record =>
                record.Status == AntenatalRecordStatus.Active
                && record.NextReminderAtUtc != null
                && record.NextReminderAtUtc <= asOfUtc)
            .OrderBy(record => record.NextReminderAtUtc)
            .ToListAsync(ct);

    public Task<AntenatalCheckupScheduleEntry?> GetScheduleEntryByIdAsync(Guid scheduleEntryId, CancellationToken ct) =>
        db.AntenatalCheckupScheduleEntries.SingleOrDefaultAsync(entry => entry.Id == scheduleEntryId, ct);

    public Task UpdateScheduleEntryAsync(AntenatalCheckupScheduleEntry entry, CancellationToken ct) =>
        db.SaveChangesAsync(ct);

    public async Task<IReadOnlyList<AntenatalRecord>> ListActiveDueForFetalMonitoringReminderAsync(
        DateTime asOfUtc,
        CancellationToken ct) =>
        await db.AntenatalRecords
            .Where(record =>
                record.Status == AntenatalRecordStatus.Active
                && record.FetalMonitoringReminderIntervalDays != null
                && record.NextFetalMonitoringReminderAtUtc != null
                && record.NextFetalMonitoringReminderAtUtc <= asOfUtc)
            .OrderBy(record => record.NextFetalMonitoringReminderAtUtc)
            .ToListAsync(ct);
}
