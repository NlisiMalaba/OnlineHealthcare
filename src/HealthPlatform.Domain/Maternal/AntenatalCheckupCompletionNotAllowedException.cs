namespace HealthPlatform.Domain.Maternal;

public sealed class AntenatalCheckupCompletionNotAllowedException(Guid scheduleEntryId)
    : Exception($"Antenatal checkup schedule entry '{scheduleEntryId}' is already completed.");
