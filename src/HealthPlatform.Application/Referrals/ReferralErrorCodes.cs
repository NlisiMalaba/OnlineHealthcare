namespace HealthPlatform.Application.Referrals;

public static class ReferralErrorCodes
{
    public const string DoctorNotFound = "DOCTOR_NOT_FOUND";
    public const string DoctorNotVerified = "DOCTOR_NOT_VERIFIED";
    public const string PatientNotFound = "PATIENT_NOT_FOUND";
    public const string ReferralNotFound = "REFERRAL_NOT_FOUND";
    public const string ReceivingDoctorNotFound = "RECEIVING_DOCTOR_NOT_FOUND";
    public const string ReferralAccessDenied = "REFERRAL_ACCESS_DENIED";
    public const string ReferralResponseNotAllowed = "REFERRAL_RESPONSE_NOT_ALLOWED";
    public const string ReferralCompletionNotAllowed = "REFERRAL_COMPLETION_NOT_ALLOWED";
    public const string ReferralAccessGrantNotFound = "REFERRAL_ACCESS_GRANT_NOT_FOUND";
    public const string HealthRecordNotFound = "HEALTH_RECORD_NOT_FOUND";
}
