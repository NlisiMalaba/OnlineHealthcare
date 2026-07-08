using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.HealthRecords.GetPatientHealthRecordEntry;

public sealed record GetPatientHealthRecordEntryQuery(string EntryId) : IQuery<HealthRecordEntryDto>;
