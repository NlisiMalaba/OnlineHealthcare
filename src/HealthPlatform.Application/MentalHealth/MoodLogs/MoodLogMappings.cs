namespace HealthPlatform.Application.MentalHealth.MoodLogs;

public static class MoodLogMappings
{
    public static MoodChartDataDto ToChartDto(
        Guid patientId,
        DateTime fromUtc,
        DateTime toUtc,
        IReadOnlyList<MoodLogDto> logs) =>
        new(
            patientId,
            fromUtc,
            toUtc,
            logs
                .OrderBy(log => log.LoggedAtUtc)
                .Select(log => new MoodChartDataPointDto(log.LoggedAtUtc, log.Rating))
                .ToList());

    public static MoodChartSharingConsentDto ToDto(this Domain.MentalHealth.MoodChartSharingConsent consent) =>
        new(
            consent.Id,
            consent.PatientId,
            consent.TherapistId,
            consent.GrantedAtUtc,
            consent.RevokedAtUtc,
            consent.IsActive);
}
