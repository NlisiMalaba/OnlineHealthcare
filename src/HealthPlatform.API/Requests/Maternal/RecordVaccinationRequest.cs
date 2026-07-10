namespace HealthPlatform.API.Requests.Maternal;

public sealed record RecordVaccinationRequest(
    Guid? ScheduleEntryId,
    string VaccineName,
    DateOnly AdministeredDate,
    string BatchNumber,
    string Provider);
