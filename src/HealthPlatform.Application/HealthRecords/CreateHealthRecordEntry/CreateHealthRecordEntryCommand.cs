using HealthPlatform.Application.Behaviors;
using HealthPlatform.Domain.HealthRecords;

namespace HealthPlatform.Application.HealthRecords.CreateHealthRecordEntry;

public sealed record CreateHealthRecordEntryCommand(
    Guid HealthRecordId,
    HealthRecordEntryType EntryType,
    HealthRecordEntryContentPayload Content,
    bool IsVisibleToPatient) : ICommand<HealthRecordEntryDto>;
