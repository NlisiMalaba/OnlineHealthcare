using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.HealthRecords.GetHealthRecordEntry;

public sealed record GetHealthRecordEntryQuery(string EntryId) : IQuery<HealthRecordEntryDto>;
