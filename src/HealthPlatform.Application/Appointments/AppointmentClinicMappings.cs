using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Appointments;

internal static class AppointmentClinicMappings
{
    public static AppointmentClinicDto? ToClinicDto(Doctor doctor, DoctorAppointmentType appointmentType)
    {
        if (!IncludesPhysicalVisit(appointmentType))
        {
            return null;
        }

        return new AppointmentClinicDto(
            doctor.ClinicAddress,
            doctor.ClinicLocation?.Latitude,
            doctor.ClinicLocation?.Longitude,
            AppointmentNavigationLinks.CreateGpsNavigationUrl(
                doctor.ClinicLocation,
                doctor.ClinicAddress));
    }

    private static bool IncludesPhysicalVisit(DoctorAppointmentType appointmentType) =>
        appointmentType is DoctorAppointmentType.Physical or DoctorAppointmentType.Both;
}
