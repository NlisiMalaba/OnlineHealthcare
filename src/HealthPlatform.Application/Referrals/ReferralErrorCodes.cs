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
}
