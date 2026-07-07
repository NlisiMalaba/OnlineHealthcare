namespace HealthPlatform.Application.Notifications;

public static class NotificationEventTypes
{
    public const string AppointmentConfirmed = "appointment.confirmed";
    public const string AppointmentReminder = "appointment.reminder";
    public const string AppointmentRescheduled = "appointment.rescheduled";
    public const string PrescriptionIssued = "prescription.issued";
    public const string PrescriptionCancelled = "prescription.cancelled";
    public const string DrugInteractionAlert = "prescription.drug_interaction_alert";
    public const string MedicationDoseReminder = "wellness.medication_dose_reminder";
    public const string MedicationScheduleCompleted = "wellness.medication_schedule_completed";
    public const string ConsecutiveMissedDoses = "wellness.consecutive_missed_doses";
    public const string OrderStatusChanged = "pharmacy.order_status_changed";
    public const string OrderPlaced = "pharmacy.order_placed";
    public const string OrderReceived = "pharmacy.order_received";
    public const string LowStockAlert = "pharmacy.low_stock_alert";
    public const string PaymentFailed = "payment.failed";
    public const string CreditBalanceWarning = "payment.credit_balance_warning";
    public const string CreditRepaymentReminder = "payment.credit_repayment_reminder";
    public const string InstalmentDueReminder = "payment.instalment_due_reminder";
    public const string InstalmentMissedPayment = "payment.instalment_missed_payment";
    public const string AccountLocked = "identity.account_locked";
    public const string DoctorLicenseVerified = "identity.doctor_license_verified";
    public const string DoctorLicenseRejected = "identity.doctor_license_rejected";
    public const string NextOfKinDesignated = "next_of_kin.designated";
    public const string EmergencyAlert = "next_of_kin.emergency_alert";
    public const string LabResultUploaded = "labs.result_uploaded";
}
