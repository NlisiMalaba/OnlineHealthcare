namespace HealthPlatform.Application.MentalHealth.MoodLogs;

public sealed record MoodLogDto(
    string Id,
    Guid PatientId,
    int Rating,
    string? Notes,
    DateTime LoggedAtUtc,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record MoodChartDataPointDto(
    DateTime LoggedAtUtc,
    int Rating);

public sealed record MoodChartDataDto(
    Guid PatientId,
    DateTime FromUtc,
    DateTime ToUtc,
    IReadOnlyList<MoodChartDataPointDto> DataPoints);

public sealed record MoodChartSharingConsentDto(
    Guid Id,
    Guid PatientId,
    Guid TherapistId,
    DateTime GrantedAtUtc,
    DateTime? RevokedAtUtc,
    bool IsActive);
