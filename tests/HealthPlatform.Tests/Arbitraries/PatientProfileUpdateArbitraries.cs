using FsCheck;
using HealthPlatform.Application.Identity.UpdatePatientProfile;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Generators;

namespace HealthPlatform.Tests.Arbitraries;

public sealed record ValidPatientProfileUpdate(
    string FullName,
    DateOnly DateOfBirth,
    BloodType BloodType,
    IReadOnlyList<string> KnownAllergies,
    IReadOnlyList<string> ChronicConditions)
{
    public UpdatePatientProfileCommand ToCommand() =>
        new(
            FullName,
            DateOfBirth,
            BloodType,
            KnownAllergies,
            ChronicConditions,
            null);
}

public sealed record ProfileUpdateRoundTripCase(
    ValidPatientRegistration Registration,
    ValidPatientProfileUpdate ProfileUpdate);

public static class PatientProfileUpdateArbitraries
{
    private static readonly string[] SampleNames =
    [
        "Jane Doe",
        "John Smith",
        "Tariro Moyo",
        "Chipo Ncube",
        "David Okonkwo"
    ];

    private static readonly string[] SampleConditions =
    [
        "Peanuts",
        "Shellfish",
        "Penicillin",
        "Latex",
        "Dust",
        "Hypertension",
        "Diabetes",
        "Asthma"
    ];

    public static Arbitrary<ProfileUpdateRoundTripCase> ProfileUpdateRoundTripCase() =>
        (from registration in PatientRegistrationArbitraries.ValidPatientRegistration().Generator
         from update in ValidPatientProfileUpdate().Generator
         select new ProfileUpdateRoundTripCase(registration, update))
        .ToArbitrary();

    public static Arbitrary<ValidPatientProfileUpdate> ValidPatientProfileUpdate() =>
        (from fullName in ValidFullName()
         from dateOfBirth in ValidDateOfBirth()
         from bloodType in DomainGenerators.EnumValues<BloodType>().Generator
         from allergies in ValidStringList()
         from conditions in ValidStringList()
         select new ValidPatientProfileUpdate(
             fullName,
             dateOfBirth,
             bloodType,
             allergies,
             conditions))
        .ToArbitrary();

    private static Gen<string> ValidFullName() =>
        from baseName in Gen.Elements(SampleNames)
        from suffix in Gen.Choose(1, 99_999)
        select $"{baseName} {suffix}";

    private static Gen<DateOnly> ValidDateOfBirth()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return
            from year in Gen.Choose(1940, today.Year)
            from month in Gen.Choose(1, 12)
            from day in Gen.Choose(1, 28)
            let candidate = new DateOnly(year, month, day)
            where candidate <= today
            select candidate;
    }

    private static Gen<List<string>> ValidStringList() =>
        from countResize in Gen.Choose(0, 10)
        from items in Gen.ArrayOf(countResize, ValidListItem())
        select items
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static Gen<string> ValidListItem() =>
        from item in Gen.Elements(SampleConditions)
        from suffix in Gen.Choose(1, 99_999)
        select $"{item}-{suffix}";
}
