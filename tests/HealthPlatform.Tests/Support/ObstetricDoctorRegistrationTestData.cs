using HealthPlatform.Application.Identity.RegisterDoctor;
using HealthPlatform.Tests.Support;

namespace HealthPlatform.Tests.Support;

public static class ObstetricDoctorRegistrationTestData
{
    public static RegisterDoctorCommand CreateValidCommand(
        string? email = null,
        string? licenseNumber = null) =>
        DoctorRegistrationTestData.CreateValidCommand(email, licenseNumber) with
        {
            Specialty = "Obstetrics and Gynecology"
        };
}
