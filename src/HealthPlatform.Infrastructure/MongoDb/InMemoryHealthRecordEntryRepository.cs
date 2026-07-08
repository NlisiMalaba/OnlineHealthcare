using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Domain.HealthRecords;

namespace HealthPlatform.Infrastructure.MongoDb;

public sealed class InMemoryHealthRecordEntryRepository : IHealthRecordEntryRepository
{
    private readonly List<StoredEntry> _entries = [];
    public List<HealthRecordReferralConsultationSummaryEntry> ReferralSummaries { get; } = [];

    public IReadOnlyList<HealthRecordEntryDto> Entries =>
        _entries
            .Where(entry => !entry.IsDeleted)
            .Select(entry => entry.Dto)
            .ToList();

    public Task<HealthRecordEntryDto> AddAsync(HealthRecordEntryCreateModel entry, CancellationToken ct)
    {
        var dto = new HealthRecordEntryDto(
            Guid.CreateVersion7().ToString("N"),
            entry.HealthRecordId,
            entry.EntryType,
            HealthRecordEntryContentResolver.Resolve(entry.EntryType, entry.Content),
            entry.AuthoredBy,
            entry.CreatedAtUtc,
            UpdatedAtUtc: null,
            entry.IsVisibleToPatient);

        _entries.Add(new StoredEntry(dto, IsDeleted: false));
        return Task.FromResult(dto);
    }

    public async Task<HealthRecordEntryReference> AddTelemedicineSessionSummaryEntryAsync(
        HealthRecordTelemedicineSummaryEntry entry,
        CancellationToken ct)
    {
        var created = await AddAsync(
            new HealthRecordEntryCreateModel(
                entry.HealthRecordId,
                HealthRecordEntryType.TelemedicineSessionSummary,
                new HealthRecordEntryContentPayload(
                    TelemedicineSessionSummary: new TelemedicineSessionSummaryContent(
                        entry.SessionId,
                        entry.AppointmentId,
                        entry.DoctorId,
                        entry.SummaryDocumentId)),
                entry.DoctorId,
                entry.CreatedAtUtc,
                IsVisibleToPatient: true),
            ct);

        return new HealthRecordEntryReference(created.Id);
    }

    public Task<HealthRecordEntryDto?> GetByIdAsync(string entryId, CancellationToken ct)
    {
        var entry = _entries.FirstOrDefault(stored => stored.Dto.Id == entryId && !stored.IsDeleted);
        return Task.FromResult(entry?.Dto);
    }

    public Task<IReadOnlyList<HealthRecordEntryDto>> ListByHealthRecordIdAsync(
        Guid healthRecordId,
        bool patientVisibleOnly,
        CancellationToken ct)
    {
        var entries = _entries
            .Where(stored => !stored.IsDeleted)
            .Select(stored => stored.Dto)
            .Where(dto => dto.HealthRecordId == healthRecordId)
            .Where(dto => !patientVisibleOnly || dto.IsVisibleToPatient)
            .OrderByDescending(dto => dto.CreatedAtUtc)
            .ToList();

        return Task.FromResult<IReadOnlyList<HealthRecordEntryDto>>(entries);
    }

    public Task<bool> UpdateAsync(HealthRecordEntryUpdateModel entry, CancellationToken ct)
    {
        var index = _entries.FindIndex(stored => stored.Dto.Id == entry.EntryId && !stored.IsDeleted);
        if (index < 0)
        {
            return Task.FromResult(false);
        }

        var existing = _entries[index].Dto;
        var mergedContent = HealthRecordEntryContentResolver.MergeForUpdate(
            existing.EntryType,
            existing.Content,
            entry.Content);

        var updated = existing with
        {
            Content = mergedContent,
            UpdatedAtUtc = entry.UpdatedAtUtc,
            IsVisibleToPatient = entry.IsVisibleToPatient ?? existing.IsVisibleToPatient
        };

        _entries[index] = _entries[index] with { Dto = updated };
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(string entryId, DateTime deletedAtUtc, CancellationToken ct)
    {
        var index = _entries.FindIndex(stored => stored.Dto.Id == entryId && !stored.IsDeleted);
        if (index < 0)
        {
            return Task.FromResult(false);
        }

        _entries[index] = _entries[index] with { IsDeleted = true };
        return Task.FromResult(true);
    }

    public Task<HealthRecordEntryReference> AddReferralConsultationSummaryEntryAsync(
        HealthRecordReferralConsultationSummaryEntry entry,
        CancellationToken ct)
    {
        ReferralSummaries.Add(entry);
        return Task.FromResult(new HealthRecordEntryReference(Guid.CreateVersion7().ToString("N")));
    }

    private sealed record StoredEntry(HealthRecordEntryDto Dto, bool IsDeleted);
}
