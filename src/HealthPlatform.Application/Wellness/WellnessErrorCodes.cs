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

    public const string HealthGoalNotFound = "HEALTH_GOAL_NOT_FOUND";

    public const string HealthGoalNotActive = "HEALTH_GOAL_NOT_ACTIVE";

    public const string WellnessEntryAccessDenied = "WELLNESS_ENTRY_ACCESS_DENIED";

    public const string HealthGoalMetricMismatch = "HEALTH_GOAL_METRIC_MISMATCH";

    public const string CarePlanNotFound = "CARE_PLAN_NOT_FOUND";

    public const string CarePlanNotActive = "CARE_PLAN_NOT_ACTIVE";

    public const string CarePlanTaskNotFound = "CARE_PLAN_TASK_NOT_FOUND";

    public const string CarePlanTaskAlreadyCompleted = "CARE_PLAN_TASK_ALREADY_COMPLETED";

    public const string CarePlanAccessDenied = "CARE_PLAN_ACCESS_DENIED";

    public const string DoctorNotVerified = "DOCTOR_NOT_VERIFIED";
}
