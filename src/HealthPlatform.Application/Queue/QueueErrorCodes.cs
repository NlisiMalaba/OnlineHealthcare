namespace HealthPlatform.Application.Queue;

public static class QueueErrorCodes
{
    public const string PatientNotFound = "PATIENT_NOT_FOUND";
    public const string AppointmentNotFound = "APPOINTMENT_NOT_FOUND";
    public const string AppointmentNotConfirmed = "APPOINTMENT_NOT_CONFIRMED";
    public const string AppointmentNotPhysical = "APPOINTMENT_NOT_PHYSICAL";
    public const string AvailabilitySlotNotFound = "AVAILABILITY_SLOT_NOT_FOUND";
    public const string DoctorNotFound = "DOCTOR_NOT_FOUND";
    public const string QueueEntryAlreadyExists = "QUEUE_ENTRY_ALREADY_EXISTS";
    public const string QueueEntryNotFound = "QUEUE_ENTRY_NOT_FOUND";
    public const string QueueActionAccessDenied = "QUEUE_ACTION_ACCESS_DENIED";
    public const string QueueEntryAlreadyClosed = "QUEUE_ENTRY_ALREADY_CLOSED";
}
