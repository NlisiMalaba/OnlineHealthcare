namespace HealthPlatform.Application.Maternal.AntenatalRecords;

public static class AntenatalRecordErrorCodes
{
    public const string PatientNotFound = "PATIENT_NOT_FOUND";
    public const string DoctorNotFound = "DOCTOR_NOT_FOUND";
    public const string DoctorNotVerified = "DOCTOR_NOT_VERIFIED";
    public const string DoctorNotObstetrician = "DOCTOR_NOT_OBSTETRICIAN";
    public const string ActiveRecordExists = "ACTIVE_ANTENATAL_RECORD_EXISTS";
    public const string AntenatalRecordNotFound = "ANTENATAL_RECORD_NOT_FOUND";
    public const string AntenatalRecordNotActive = "ANTENATAL_RECORD_NOT_ACTIVE";
    public const string ObstetricDoctorAccessDenied = "OBSTETRIC_DOCTOR_ACCESS_DENIED";
    public const string ScheduleEntryNotFound = "SCHEDULE_ENTRY_NOT_FOUND";
    public const string ScheduleEntryMismatch = "SCHEDULE_ENTRY_MISMATCH";
    public const string CheckupAlreadyCompleted = "CHECKUP_ALREADY_COMPLETED";
    public const string InvalidFetalMonitoringInterval = "INVALID_FETAL_MONITORING_INTERVAL";
}
