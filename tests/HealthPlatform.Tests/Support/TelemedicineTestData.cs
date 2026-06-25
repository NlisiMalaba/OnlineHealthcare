using HealthPlatform.Application.Identity.RegisterDoctor;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;

namespace HealthPlatform.Tests.Support;

public static class TelemedicineTestData
{
    public static RegisterDoctorCommand CreateVirtualDoctorCommand(
        string? email = null,
        string? licenseNumber = null)
    {
        var credentialsStream = new MemoryStream([0x25, 0x50, 0x44, 0x46, 0x2D]);
        var credentials = new DoctorFileUpload(
            credentialsStream,
            "application/pdf",
            "license.pdf",
            credentialsStream.Length);

        return new RegisterDoctorCommand(
            "Dr. Virtual Moyo",
            licenseNumber ?? $"HPCZ-{Guid.NewGuid():N}"[..20],
            "General Practice",
            8,
            "12 Samora Machel Ave, Harare",
            -17.8252,
            31.0335,
            25m,
            40m,
            "Virtual consultations only.",
            email ?? $"virtual-doctor-{Guid.NewGuid():N}@example.com",
            $"+2637{Random.Shared.Next(10_000_000, 99_999_999)}",
            PatientRegistrationTestHost.ValidPassword,
            [
                new DoctorAvailabilitySlotInput(
                    DayOfWeek.Monday,
                    new TimeOnly(8, 0),
                    new TimeOnly(12, 0),
                    30,
                    DoctorAppointmentType.Virtual)
            ],
            null,
            credentials);
    }

    public static RegisterDoctorCommand CreatePhysicalOnlyDoctorCommand(
        string? email = null,
        string? licenseNumber = null)
    {
        var credentialsStream = new MemoryStream([0x25, 0x50, 0x44, 0x46, 0x2D]);
        var credentials = new DoctorFileUpload(
            credentialsStream,
            "application/pdf",
            "license.pdf",
            credentialsStream.Length);

        return new RegisterDoctorCommand(
            "Dr. Physical Moyo",
            licenseNumber ?? $"HPCZ-{Guid.NewGuid():N}"[..20],
            "General Practice",
            8,
            "12 Samora Machel Ave, Harare",
            -17.8252,
            31.0335,
            25m,
            40m,
            "Clinic visits only.",
            email ?? $"physical-doctor-{Guid.NewGuid():N}@example.com",
            $"+2637{Random.Shared.Next(10_000_000, 99_999_999)}",
            PatientRegistrationTestHost.ValidPassword,
            [
                new DoctorAvailabilitySlotInput(
                    DayOfWeek.Monday,
                    new TimeOnly(8, 0),
                    new TimeOnly(12, 0),
                    30,
                    DoctorAppointmentType.Physical)
            ],
            null,
            credentials);
    }
}
