namespace HealthPlatform.Application.Telemedicine;

public static class TelemedicineErrorCodes
{
    public const string SessionNotFound = "TELEMEDICINE_SESSION_NOT_FOUND";
    public const string AppointmentNotFound = "APPOINTMENT_NOT_FOUND";
    public const string PatientNotFound = "PATIENT_NOT_FOUND";
    public const string DoctorNotFound = "DOCTOR_NOT_FOUND";
    public const string NotVirtualAppointment = "NOT_VIRTUAL_APPOINTMENT";
    public const string AppointmentNotConfirmed = "APPOINTMENT_NOT_CONFIRMED";
    public const string SessionNotJoinable = "TELEMEDICINE_SESSION_NOT_JOINABLE";
    public const string AvailabilitySlotNotFound = "AVAILABILITY_SLOT_NOT_FOUND";
    public const string RecordingConsentRequired = "RECORDING_CONSENT_REQUIRED";
    public const string RecordingConsentNotAllowed = "RECORDING_CONSENT_NOT_ALLOWED";
    public const string SessionNotActive = "TELEMEDICINE_SESSION_NOT_ACTIVE";
    public const string ChatMessageEmpty = "TELEMEDICINE_CHAT_MESSAGE_EMPTY";
    public const string FileShareNotAllowed = "TELEMEDICINE_FILE_SHARE_NOT_ALLOWED";
    public const string UnsupportedFileType = "TELEMEDICINE_UNSUPPORTED_FILE_TYPE";
}
