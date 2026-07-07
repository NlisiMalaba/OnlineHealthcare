namespace HealthPlatform.Application.Labs;

public static class LabOrderErrorCodes
{
    public const string LabOrderNotFound = "LAB_ORDER_NOT_FOUND";
    public const string PatientNotFound = "PATIENT_NOT_FOUND";
    public const string HealthRecordNotFound = "HEALTH_RECORD_NOT_FOUND";
    public const string DoctorNotFound = "DOCTOR_NOT_FOUND";
    public const string DoctorNotVerified = "DOCTOR_NOT_VERIFIED";
    public const string HealthRecordOwnershipMismatch = "HEALTH_RECORD_OWNERSHIP_MISMATCH";
    public const string LabOrderReferenceNotFound = "LAB_ORDER_REFERENCE_NOT_FOUND";
}
