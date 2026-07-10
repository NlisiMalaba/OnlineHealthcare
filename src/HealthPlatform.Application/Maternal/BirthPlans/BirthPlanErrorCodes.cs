namespace HealthPlatform.Application.Maternal.BirthPlans;

public static class BirthPlanErrorCodes
{
    public const string PatientNotFound = "PATIENT_NOT_FOUND";
    public const string DoctorNotFound = "DOCTOR_NOT_FOUND";
    public const string AntenatalRecordNotFound = "ANTENATAL_RECORD_NOT_FOUND";
    public const string AntenatalRecordNotActive = "ANTENATAL_RECORD_NOT_ACTIVE";
    public const string BirthPlanNotFound = "BIRTH_PLAN_NOT_FOUND";
    public const string BirthPlanAlreadyExists = "BIRTH_PLAN_ALREADY_EXISTS";
    public const string AccessDenied = "MATERNAL_CARE_ACCESS_DENIED";
    public const string AccessAlreadyGranted = "MATERNAL_CARE_ACCESS_ALREADY_GRANTED";
    public const string AccessGrantNotFound = "MATERNAL_CARE_ACCESS_GRANT_NOT_FOUND";
}
