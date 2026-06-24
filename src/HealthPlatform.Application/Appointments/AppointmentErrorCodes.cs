namespace HealthPlatform.Application.Appointments;

public static class AppointmentErrorCodes
{
    public const string DoctorNotFound = "DOCTOR_NOT_FOUND";
    public const string AvailabilitySlotNotFound = "AVAILABILITY_SLOT_NOT_FOUND";
    public const string AvailabilitySlotConflict = "AVAILABILITY_SLOT_CONFLICT";
    public const string PatientNotFound = "PATIENT_NOT_FOUND";
    public const string SlotUnavailable = "SLOT_UNAVAILABLE";
    public const string AppointmentNotFound = "APPOINTMENT_NOT_FOUND";
    public const string AppointmentNotCancellable = "APPOINTMENT_NOT_CANCELLABLE";
    public const string AppointmentNotReschedulable = "APPOINTMENT_NOT_RESCHEDULABLE";
}
