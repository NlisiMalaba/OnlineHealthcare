using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Queue;

public static class PhysicalAppointmentRules
{
    public static bool SupportsVirtualQueue(DoctorAppointmentType appointmentType) =>
        appointmentType is DoctorAppointmentType.Physical or DoctorAppointmentType.Both;
}
