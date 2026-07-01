namespace HealthPlatform.Application.Wellness;

public static class WellnessErrorCodes
{
    public const string PatientNotFound = "PATIENT_NOT_FOUND";

    public const string ScheduleNotFound = "MEDICATION_SCHEDULE_NOT_FOUND";

    public const string DoseNotOnSchedule = "DOSE_NOT_ON_SCHEDULE";

    public const string DoseAlreadyRecorded = "DOSE_ALREADY_RECORDED";

    public const string DoseConfirmationWindowExpired = "DOSE_CONFIRMATION_WINDOW_EXPIRED";

    public const string CannotConfirmFutureDose = "CANNOT_CONFIRM_FUTURE_DOSE";

    public const string DoctorNotFound = "DOCTOR_NOT_FOUND";

    public const string ScheduleNotActive = "MEDICATION_SCHEDULE_NOT_ACTIVE";

    public const string AdherenceSummaryAccessDenied = "ADHERENCE_SUMMARY_ACCESS_DENIED";
}
