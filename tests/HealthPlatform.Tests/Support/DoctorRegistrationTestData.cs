using HealthPlatform.Application.Identity.RegisterDoctor;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;

namespace HealthPlatform.Tests.Support;

public static class DoctorRegistrationTestData
{
    public static RegisterDoctorCommand CreateValidCommand(
        string? email = null,
        string? licenseNumber = null,
        string? phoneNumber = null)
    {
        var credentialsStream = new MemoryStream([0x25, 0x50, 0x44, 0x46, 0x2D]);
        var credentials = new DoctorFileUpload(
            credentialsStream,
            "application/pdf",
            "license.pdf",
            credentialsStream.Length);

        return new RegisterDoctorCommand(
            "Dr. Tendai Moyo",
            licenseNumber ?? $"HPCZ-{Guid.NewGuid():N}"[..20],
            "General Practice",
            8,
            "12 Samora Machel Ave, Harare",
            -17.8252,
            31.0335,
            25m,
            40m,
            "Experienced general practitioner.",
            email ?? $"doctor-{Guid.NewGuid():N}@example.com",
            phoneNumber ?? $"+2637{Random.Shared.Next(10_000_000, 99_999_999)}",
            PatientRegistrationTestHost.ValidPassword,
            [
                new DoctorAvailabilitySlotInput(
                    DayOfWeek.Monday,
                    new TimeOnly(8, 0),
                    new TimeOnly(12, 0),
                    30,
                    DoctorAppointmentType.Both)
            ],
            null,
            credentials);
    }
}
