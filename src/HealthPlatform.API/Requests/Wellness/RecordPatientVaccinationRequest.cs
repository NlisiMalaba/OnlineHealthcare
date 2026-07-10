namespace HealthPlatform.API.Requests.Wellness;

public sealed record RecordPatientVaccinationRequest(
    Guid? PatientId,
    Guid? ScheduleEntryId,
    string VaccineName,
    DateOnly AdministeredDate,
    string BatchNumber,
    string Provider);
