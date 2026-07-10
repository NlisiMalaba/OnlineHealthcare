namespace HealthPlatform.Application.Maternal.AntenatalRecords;

public static class AntenatalRecordErrorCodes
{
    public const string PatientNotFound = "PATIENT_NOT_FOUND";
    public const string DoctorNotFound = "DOCTOR_NOT_FOUND";
    public const string DoctorNotVerified = "DOCTOR_NOT_VERIFIED";
    public const string DoctorNotObstetrician = "DOCTOR_NOT_OBSTETRICIAN";
    public const string ActiveRecordExists = "ACTIVE_ANTENATAL_RECORD_EXISTS";
}
