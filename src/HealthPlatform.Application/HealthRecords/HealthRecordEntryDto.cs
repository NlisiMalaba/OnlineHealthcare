using HealthPlatform.Domain.HealthRecords;

namespace HealthPlatform.Application.HealthRecords;

public sealed record HealthRecordEntryDto(
    string Id,
    Guid HealthRecordId,
    HealthRecordEntryType EntryType,
    HealthRecordEntryContentPayload Content,
    Guid AuthoredBy,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    bool IsVisibleToPatient);
