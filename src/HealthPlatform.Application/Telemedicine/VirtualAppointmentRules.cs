using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Telemedicine;

public static class VirtualAppointmentRules
{
    public static bool SupportsTelemedicine(DoctorAppointmentType appointmentType) =>
        appointmentType is DoctorAppointmentType.Virtual or DoctorAppointmentType.Both;
}
