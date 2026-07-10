using HealthPlatform.Application.MentalHealth.MoodLogs;

namespace HealthPlatform.Infrastructure.MongoDb;

public sealed class InMemoryMoodLogRepository : IMoodLogRepository
{
    private readonly List<StoredMoodLog> _logs = [];

    public IReadOnlyList<MoodLogDto> Logs =>
        _logs.Where(log => !log.IsDeleted).Select(log => log.Dto).ToList();

    public Task<MoodLogDto> AddAsync(MoodLogCreateModel entry, CancellationToken ct)
    {
        var dto = new MoodLogDto(
            Guid.CreateVersion7().ToString("N"),
            entry.PatientId,
            entry.Rating,
            entry.Notes,
            entry.LoggedAtUtc,
            entry.CreatedAtUtc,
            UpdatedAtUtc: null);

        _logs.Add(new StoredMoodLog(dto, IsDeleted: false));
        return Task.FromResult(dto);
    }

    public Task<MoodLogDto?> GetByIdAsync(string id, CancellationToken ct) =>
        Task.FromResult(_logs.FirstOrDefault(log => log.Dto.Id == id && !log.IsDeleted)?.Dto);

    public Task<MoodLogDto?> GetByIdForPatientAsync(string id, Guid patientId, CancellationToken ct) =>
        Task.FromResult(_logs.FirstOrDefault(
            log => log.Dto.Id == id && log.Dto.PatientId == patientId && !log.IsDeleted)?.Dto);

    public Task<IReadOnlyList<MoodLogDto>> ListByPatientIdAsync(
        Guid patientId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken ct)
    {
        var logs = _logs
            .Where(log => !log.IsDeleted)
            .Select(log => log.Dto)
            .Where(log => log.PatientId == patientId)
            .Where(log => !fromUtc.HasValue || log.LoggedAtUtc >= fromUtc.Value)
            .Where(log => !toUtc.HasValue || log.LoggedAtUtc <= toUtc.Value)
            .OrderByDescending(log => log.LoggedAtUtc)
            .ToList();

        return Task.FromResult<IReadOnlyList<MoodLogDto>>(logs);
    }

    public Task<bool> UpdateAsync(MoodLogUpdateModel update, CancellationToken ct)
    {
        var index = _logs.FindIndex(log => log.Dto.Id == update.Id && !log.IsDeleted);
        if (index < 0)
        {
            return Task.FromResult(false);
        }

        var existing = _logs[index].Dto;
        _logs[index] = _logs[index] with
        {
            Dto = existing with
            {
                Rating = update.Rating,
                Notes = update.Notes,
                LoggedAtUtc = update.LoggedAtUtc,
                UpdatedAtUtc = update.UpdatedAtUtc
            }
        };

        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(string id, DateTime deletedAtUtc, CancellationToken ct)
    {
        var index = _logs.FindIndex(log => log.Dto.Id == id && !log.IsDeleted);
        if (index < 0)
        {
            return Task.FromResult(false);
        }

        _logs[index] = _logs[index] with { IsDeleted = true };
        return Task.FromResult(true);
    }

    private sealed record StoredMoodLog(MoodLogDto Dto, bool IsDeleted);
}
