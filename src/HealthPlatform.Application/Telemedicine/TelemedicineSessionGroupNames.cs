namespace HealthPlatform.Application.Telemedicine;

public static class TelemedicineSessionGroupNames
{
    public static string ForAppointment(Guid appointmentId) => $"telemedicine-session:{appointmentId:N}";
}
