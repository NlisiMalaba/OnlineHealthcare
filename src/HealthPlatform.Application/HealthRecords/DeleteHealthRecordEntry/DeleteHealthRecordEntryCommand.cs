using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.HealthRecords.DeleteHealthRecordEntry;

public sealed record DeleteHealthRecordEntryCommand(string EntryId) : ICommand;
