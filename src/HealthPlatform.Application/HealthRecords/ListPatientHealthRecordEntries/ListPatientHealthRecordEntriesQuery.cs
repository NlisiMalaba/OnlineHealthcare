using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.HealthRecords.ListPatientHealthRecordEntries;

public sealed record ListPatientHealthRecordEntriesQuery() : IQuery<IReadOnlyList<HealthRecordEntryDto>>;
