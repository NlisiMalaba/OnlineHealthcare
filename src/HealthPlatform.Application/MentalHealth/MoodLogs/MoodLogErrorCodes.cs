namespace HealthPlatform.Application.MentalHealth.MoodLogs;

public static class MoodLogErrorCodes
{
    public const string MoodLogNotFound = "MOOD_LOG_NOT_FOUND";
    public const string PatientNotFound = "PATIENT_NOT_FOUND";
    public const string DoctorNotFound = "DOCTOR_NOT_FOUND";
    public const string PatientAccessDenied = "PATIENT_ACCESS_DENIED";
    public const string TherapistAccessDenied = "THERAPIST_ACCESS_DENIED";
    public const string TherapistRequired = "THERAPIST_REQUIRED";
    public const string MoodChartConsentRequired = "MOOD_CHART_CONSENT_REQUIRED";
    public const string MoodChartConsentNotFound = "MOOD_CHART_CONSENT_NOT_FOUND";
    public const string InvalidRating = "INVALID_MOOD_RATING";
}
