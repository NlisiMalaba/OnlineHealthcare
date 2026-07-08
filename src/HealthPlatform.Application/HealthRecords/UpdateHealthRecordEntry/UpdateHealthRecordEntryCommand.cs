using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.HealthRecords.UpdateHealthRecordEntry;

public sealed record UpdateHealthRecordEntryCommand(
    string EntryId,
    HealthRecordEntryContentPayload Content,
    bool? IsVisibleToPatient) : ICommand<HealthRecordEntryDto>;
