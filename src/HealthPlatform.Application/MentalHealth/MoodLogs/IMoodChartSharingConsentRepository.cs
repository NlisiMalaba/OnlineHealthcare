using HealthPlatform.Domain.MentalHealth;

namespace HealthPlatform.Application.MentalHealth.MoodLogs;

public interface IMoodChartSharingConsentRepository
{
    Task<MoodChartSharingConsent?> GetActiveConsentAsync(
        Guid patientId,
        Guid therapistId,
        CancellationToken ct);

    Task<MoodChartSharingConsent?> GetLatestConsentAsync(
        Guid patientId,
        Guid therapistId,
        CancellationToken ct);

    Task AddAsync(MoodChartSharingConsent consent, CancellationToken ct);

    Task UpdateAsync(MoodChartSharingConsent consent, CancellationToken ct);
}
