namespace HealthPlatform.Application.HealthRecords;

public static class HealthRecordErrorCodes
{
    public const string HealthRecordNotFound = "HEALTH_RECORD_NOT_FOUND";

    public const string HealthRecordEntryNotFound = "HEALTH_RECORD_ENTRY_NOT_FOUND";

    public const string InvalidEntryContent = "INVALID_ENTRY_CONTENT";

    public const string DoctorNotFound = "DOCTOR_NOT_FOUND";

    public const string DoctorNotVerified = "DOCTOR_NOT_VERIFIED";

    public const string PatientNotFound = "PATIENT_NOT_FOUND";

    public const string HealthRecordAccessAlreadyGranted = "HEALTH_RECORD_ACCESS_ALREADY_GRANTED";

    public const string HealthRecordAccessNotFound = "HEALTH_RECORD_ACCESS_NOT_FOUND";
}
