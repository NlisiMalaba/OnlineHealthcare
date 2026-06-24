namespace HealthPlatform.Application.Appointments;

public static class AppointmentErrorCodes
{
    public const string DoctorNotFound = "DOCTOR_NOT_FOUND";
    public const string AvailabilitySlotNotFound = "AVAILABILITY_SLOT_NOT_FOUND";
    public const string AvailabilitySlotConflict = "AVAILABILITY_SLOT_CONFLICT";
}
