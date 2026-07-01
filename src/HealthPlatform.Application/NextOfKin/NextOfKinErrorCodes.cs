namespace HealthPlatform.Application.NextOfKin;

public static class NextOfKinErrorCodes
{
    public const string PatientNotFound = "PATIENT_NOT_FOUND";

    public const string ContactNotFound = "NEXT_OF_KIN_CONTACT_NOT_FOUND";

    public const string MaxContactsReached = "NEXT_OF_KIN_MAX_CONTACTS_REACHED";

    public const string DoctorNotFound = "DOCTOR_NOT_FOUND";

    public const string DoctorNotVerified = "DOCTOR_NOT_VERIFIED";

    public const string AppointmentNotFound = "APPOINTMENT_NOT_FOUND";

    public const string AppointmentNotEligibleForEmergencyAlert = "APPOINTMENT_NOT_ELIGIBLE_FOR_EMERGENCY_ALERT";
}
