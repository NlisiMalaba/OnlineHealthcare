namespace HealthPlatform.Application.Vaccinations;

public static class VaccinationErrorCodes
{
    public const string PatientNotFound = "PATIENT_NOT_FOUND";
    public const string ChildProfileNotFound = "CHILD_PROFILE_NOT_FOUND";
    public const string ScheduleEntryNotFound = "VACCINATION_SCHEDULE_ENTRY_NOT_FOUND";
    public const string AccessDenied = "VACCINATION_ACCESS_DENIED";
    public const string ScheduleEntryMismatch = "VACCINATION_SCHEDULE_ENTRY_MISMATCH";
    public const string ScheduleEntryCompleted = "VACCINATION_SCHEDULE_ENTRY_COMPLETED";
}
