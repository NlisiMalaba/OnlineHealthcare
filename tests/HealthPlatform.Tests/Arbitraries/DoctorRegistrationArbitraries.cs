using FsCheck;
using HealthPlatform.Application.Identity.RegisterDoctor;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Generators;
using HealthPlatform.Tests.Support;

namespace HealthPlatform.Tests.Arbitraries;

public sealed record ValidDoctorRegistration(
    string FullName,
    string LicenseNumber,
    string Specialty,
    int YearsOfExperience,
    string ClinicAddress,
    double? ClinicLatitude,
    double? ClinicLongitude,
    decimal VirtualFee,
    decimal PhysicalFee,
    string? Bio,
    string Email,
    string PhoneNumber,
    IReadOnlyList<DoctorAvailabilitySlotInput> AvailabilitySlots,
    bool IncludeProfilePhoto)
{
    public RegisterDoctorCommand ToCommand()
    {
        var credentialsStream = new MemoryStream([0x25, 0x50, 0x44, 0x46, 0x2D]);
        var credentials = new DoctorFileUpload(
            credentialsStream,
            "application/pdf",
            "credentials.pdf",
            credentialsStream.Length);

        DoctorFileUpload? photo = null;
        if (IncludeProfilePhoto)
        {
            var photoStream = new MemoryStream([0xFF, 0xD8, 0xFF, 0xD9]);
            photo = new DoctorFileUpload(
                photoStream,
                "image/jpeg",
                "profile.jpg",
                photoStream.Length);
        }

        return new RegisterDoctorCommand(
            FullName,
            LicenseNumber,
            Specialty,
            YearsOfExperience,
            ClinicAddress,
            ClinicLatitude,
            ClinicLongitude,
            VirtualFee,
            PhysicalFee,
            Bio,
            Email,
            PhoneNumber,
            PatientRegistrationTestHost.ValidPassword,
            AvailabilitySlots,
            photo,
            credentials);
    }
}

public static class DoctorRegistrationArbitraries
{
    private static readonly string[] SampleNames =
    [
        "Dr. Jane Doe",
        "Dr. John Smith",
        "Dr. Tariro Moyo",
        "Dr. Chipo Ncube",
        "Dr. David Okonkwo"
    ];

    private static readonly string[] SampleSpecialties =
    [
        "General Practice",
        "Cardiology",
        "Pediatrics",
        "Dermatology",
        "Obstetrics"
    ];

    private static readonly string[] SampleAddresses =
    [
        "12 Samora Machel Ave, Harare",
        "45 Leopold Takawira, Bulawayo",
        "8 Jason Moyo Ave, Harare",
        "22 Fife Street, Bulawayo"
    ];

    public static Arbitrary<ValidDoctorRegistration> ValidDoctorRegistration() =>
        (from fullName in ValidFullName()
         from licenseNumber in ValidLicenseNumber()
         from specialty in Gen.Elements(SampleSpecialties)
         from yearsOfExperience in Gen.Choose(0, 45)
         from clinicAddress in Gen.Elements(SampleAddresses)
         from location in OptionalClinicLocation()
         from virtualFee in Gen.Choose(0, 500)
         from physicalFee in Gen.Choose(0, 800)
         from bio in OptionalBio()
         from email in ValidEmail()
         from phoneNumber in ValidPhoneNumber()
         from slots in ValidAvailabilitySlots()
         from includePhoto in Gen.OneOf(Gen.Constant(true), Gen.Constant(false))
         select new ValidDoctorRegistration(
             fullName,
             licenseNumber,
             specialty,
             yearsOfExperience,
             clinicAddress,
             location?.Latitude,
             location?.Longitude,
             virtualFee,
             physicalFee,
             bio,
             email,
             phoneNumber,
             slots,
             includePhoto))
        .ToArbitrary();

    private static Gen<string> ValidFullName() =>
        from baseName in Gen.Elements(SampleNames)
        from suffix in Gen.Choose(1, 99_999)
        select $"{baseName} {suffix}";

    private static Gen<string> ValidLicenseNumber() =>
        from unique in Arb.Default.Guid().Generator.Where(g => g != Guid.Empty)
        select $"HPCZ-{unique:N}"[..20];

    private static Gen<(double Latitude, double Longitude)?> OptionalClinicLocation() =>
        Gen.OneOf(
            Gen.Constant<(double Latitude, double Longitude)?>(null),
            DomainGenerators.WildGeoPoint().Generator
                .Select(point => ((double Latitude, double Longitude)?)(point.Latitude, point.Longitude)));

    private static Gen<string?> OptionalBio() =>
        Gen.OneOf(
            Gen.Constant<string?>(null),
            from prefix in Gen.Elements("Experienced", "Dedicated", "Compassionate")
            from suffix in Gen.Choose(1, 9999)
            select $"{prefix} clinician #{suffix}.");

    private static Gen<string> ValidEmail() =>
        from unique in Arb.Default.Guid().Generator.Where(g => g != Guid.Empty)
        select $"doctor-{unique:N}@example.com";

    private static Gen<string> ValidPhoneNumber() =>
        from suffix in Gen.Choose(10_000_000, 99_999_999)
        select $"+2637{suffix}";

    private static Gen<List<DoctorAvailabilitySlotInput>> ValidAvailabilitySlots() =>
        from count in Gen.Choose(1, 3)
        from slots in Gen.ArrayOf(count, ValidAvailabilitySlot())
        select slots.ToList();

    private static Gen<DoctorAvailabilitySlotInput> ValidAvailabilitySlot() =>
        from day in DomainGenerators.EnumValues<DayOfWeek>().Generator
        from startHour in Gen.Choose(6, 10)
        from durationHours in Gen.Choose(2, 6)
        from slotDuration in Gen.Elements(15, 30, 45, 60)
        from appointmentType in DomainGenerators.EnumValues<DoctorAppointmentType>().Generator
        let start = new TimeOnly(startHour, 0)
        let end = start.AddHours(durationHours)
        select new DoctorAvailabilitySlotInput(day, start, end, slotDuration, appointmentType);
}
