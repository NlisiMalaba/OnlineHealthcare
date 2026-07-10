using HealthPlatform.Application.Vaccinations;
using HealthPlatform.Domain.Vaccinations;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class VaccinationScheduleRepository(ApplicationDbContext db) : IVaccinationScheduleRepository
{
    public async Task AddRangeAsync(IReadOnlyCollection<VaccinationScheduleEntry> entries, CancellationToken ct)
    {
        await db.VaccinationScheduleEntries.AddRangeAsync(entries, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task<bool> HasScheduleForChildAsync(Guid childProfileId, CancellationToken ct) =>
        db.VaccinationScheduleEntries.AnyAsync(entry => entry.ChildProfileId == childProfileId, ct);

    public Task<bool> HasScheduleForPatientAsync(Guid patientId, CancellationToken ct) =>
        db.VaccinationScheduleEntries.AnyAsync(entry => entry.PatientId == patientId, ct);

    public async Task<IReadOnlyList<VaccinationScheduleEntry>> ListByChildProfileIdAsync(
        Guid childProfileId,
        CancellationToken ct) =>
        await db.VaccinationScheduleEntries
            .Where(entry => entry.ChildProfileId == childProfileId)
            .OrderBy(entry => entry.RecommendedDate)
            .ThenBy(entry => entry.VaccineName)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<VaccinationScheduleEntry>> ListByPatientIdAsync(
        Guid patientId,
        CancellationToken ct) =>
        await db.VaccinationScheduleEntries
            .Where(entry => entry.PatientId == patientId)
            .OrderBy(entry => entry.RecommendedDate)
            .ThenBy(entry => entry.VaccineName)
            .ToListAsync(ct);

    public Task<VaccinationScheduleEntry?> GetByIdAsync(Guid scheduleEntryId, CancellationToken ct) =>
        db.VaccinationScheduleEntries.SingleOrDefaultAsync(entry => entry.Id == scheduleEntryId, ct);

    public async Task<IReadOnlyList<VaccinationScheduleEntry>> ListDueForReminderAsync(
        DateOnly asOfDate,
        CancellationToken ct)
    {
        var reminderWindowEnd = asOfDate.AddDays(VaccinationReminderPolicies.ReminderLeadDays);

        return await db.VaccinationScheduleEntries
            .Where(entry =>
                entry.CompletedAtUtc == null
                && entry.ReminderSentAtUtc == null
                && entry.RecommendedDate >= asOfDate
                && entry.RecommendedDate <= reminderWindowEnd)
            .OrderBy(entry => entry.RecommendedDate)
            .ToListAsync(ct);
    }

    public Task UpdateAsync(VaccinationScheduleEntry entry, CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
