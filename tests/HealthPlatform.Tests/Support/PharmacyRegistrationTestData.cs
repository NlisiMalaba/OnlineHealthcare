using HealthPlatform.Application.Identity.RegisterPharmacy;

namespace HealthPlatform.Tests.Support;

public static class PharmacyRegistrationTestData
{
    public static RegisterPharmacyCommand CreateValidCommand(
        string? email = null,
        string? phoneNumber = null,
        string? name = null)
    {
        return new RegisterPharmacyCommand(
            name ?? $"Pharmacy {Guid.NewGuid():N}"[..24],
            "45 Jason Moyo Ave, Harare",
            -17.8292,
            31.0522,
            email ?? $"pharmacy-{Guid.NewGuid():N}@example.com",
            phoneNumber ?? $"+2637{Random.Shared.Next(10_000_000, 99_999_999)}",
            PatientRegistrationTestHost.ValidPassword,
            null);
    }
}
