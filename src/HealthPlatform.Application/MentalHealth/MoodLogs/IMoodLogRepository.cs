namespace HealthPlatform.Application.MentalHealth.MoodLogs;

public sealed record MoodLogCreateModel(
    Guid PatientId,
    int Rating,
    string? Notes,
    DateTime LoggedAtUtc,
    DateTime CreatedAtUtc);

public sealed record MoodLogUpdateModel(
    string Id,
    int Rating,
    string? Notes,
    DateTime LoggedAtUtc,
    DateTime UpdatedAtUtc);

public interface IMoodLogRepository
{
    Task<MoodLogDto> AddAsync(MoodLogCreateModel entry, CancellationToken ct);

    Task<MoodLogDto?> GetByIdAsync(string id, CancellationToken ct);

    Task<MoodLogDto?> GetByIdForPatientAsync(string id, Guid patientId, CancellationToken ct);

    Task<IReadOnlyList<MoodLogDto>> ListByPatientIdAsync(
        Guid patientId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken ct);

    Task<IReadOnlyList<MoodLogDto>> ListRecentByPatientIdAsync(
        Guid patientId,
        int count,
        CancellationToken ct);

    Task<bool> UpdateAsync(MoodLogUpdateModel update, CancellationToken ct);

    Task<bool> DeleteAsync(string id, DateTime deletedAtUtc, CancellationToken ct);
}
