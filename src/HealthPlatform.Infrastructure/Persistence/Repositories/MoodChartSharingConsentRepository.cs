using HealthPlatform.Application.MentalHealth.MoodLogs;
using HealthPlatform.Domain.MentalHealth;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class MoodChartSharingConsentRepository(ApplicationDbContext db)
    : IMoodChartSharingConsentRepository
{
    public async Task<MoodChartSharingConsent?> GetActiveConsentAsync(
        Guid patientId,
        Guid therapistId,
        CancellationToken ct) =>
        await db.MoodChartSharingConsents
            .Where(consent =>
                consent.PatientId == patientId
                && consent.TherapistId == therapistId
                && consent.RevokedAtUtc == null)
            .OrderByDescending(consent => consent.GrantedAtUtc)
            .FirstOrDefaultAsync(ct);

    public Task<MoodChartSharingConsent?> GetLatestConsentAsync(
        Guid patientId,
        Guid therapistId,
        CancellationToken ct) =>
        db.MoodChartSharingConsents
            .Where(consent => consent.PatientId == patientId && consent.TherapistId == therapistId)
            .OrderByDescending(consent => consent.GrantedAtUtc)
            .FirstOrDefaultAsync(ct);

    public async Task AddAsync(MoodChartSharingConsent consent, CancellationToken ct)
    {
        await db.MoodChartSharingConsents.AddAsync(consent, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task UpdateAsync(MoodChartSharingConsent consent, CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
