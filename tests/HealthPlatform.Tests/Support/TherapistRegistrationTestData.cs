using HealthPlatform.Application.Identity.RegisterDoctor;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;

namespace HealthPlatform.Tests.Support;

public static class TherapistRegistrationTestData
{
    public static RegisterDoctorCommand CreateValidCommand(
        string? email = null,
        string? licenseNumber = null) =>
        DoctorRegistrationTestData.CreateValidCommand(email, licenseNumber) with
        {
            Specialty = "Clinical Psychology"
        };
}
