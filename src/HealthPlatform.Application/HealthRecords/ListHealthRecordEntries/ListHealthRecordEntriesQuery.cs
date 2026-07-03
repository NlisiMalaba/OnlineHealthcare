using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.HealthRecords.ListHealthRecordEntries;

public sealed record ListHealthRecordEntriesQuery(Guid HealthRecordId) : IQuery<IReadOnlyList<HealthRecordEntryDto>>;
