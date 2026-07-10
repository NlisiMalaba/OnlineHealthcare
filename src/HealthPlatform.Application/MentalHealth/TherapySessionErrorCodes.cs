namespace HealthPlatform.Application.MentalHealth;

public static class TherapySessionErrorCodes
{
    public const string TherapySessionNotFound = "THERAPY_SESSION_NOT_FOUND";
    public const string TherapistRequired = "THERAPIST_REQUIRED";
    public const string TherapistAccessDenied = "THERAPIST_ACCESS_DENIED";
    public const string PatientAccessDenied = "PATIENT_ACCESS_DENIED";
    public const string AppointmentNotFound = "APPOINTMENT_NOT_FOUND";
    public const string HealthRecordNotFound = "HEALTH_RECORD_NOT_FOUND";
    public const string DoctorNotFound = "DOCTOR_NOT_FOUND";
    public const string PatientNotFound = "PATIENT_NOT_FOUND";
    public const string TherapySessionCompletionNotAllowed = "THERAPY_SESSION_COMPLETION_NOT_ALLOWED";
    public const string TherapySessionBroaderAccessNotAllowed = "THERAPY_SESSION_BROADER_ACCESS_NOT_ALLOWED";
    public const string InvalidConsultationType = "INVALID_CONSULTATION_TYPE";
}
