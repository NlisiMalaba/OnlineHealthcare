using FsCheck;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;

namespace HealthPlatform.Tests.Arbitraries;

public sealed record ValidPatientRegistration(
    PatientAuthProvider AuthProvider,
    string FullName,
    string? PhoneNumber,
    string? Email,
    string? Password,
    string? IdToken)
{
    public RegisterPatientCommand ToCommand() =>
        new(AuthProvider, FullName, PhoneNumber, Email, Password, IdToken);
}

public enum DuplicateIdentifierKind
{
    Phone = 0,
    Email = 1
}

public sealed record DuplicateRegistrationCase(
    DuplicateIdentifierKind Kind,
    string FirstFullName,
    string SecondFullName,
    string Identifier)
{
    public RegisterPatientCommand FirstCommand() =>
        Kind switch
        {
            DuplicateIdentifierKind.Phone => new RegisterPatientCommand(
                PatientAuthProvider.Phone,
                FirstFullName,
                Identifier,
                null,
                PatientRegistrationTestHost.ValidPassword,
                null),
            DuplicateIdentifierKind.Email => new RegisterPatientCommand(
                PatientAuthProvider.Email,
                FirstFullName,
                null,
                Identifier,
                PatientRegistrationTestHost.ValidPassword,
                null),
            _ => throw new InvalidOperationException("Unsupported duplicate identifier kind.")
        };

    public RegisterPatientCommand SecondCommand() =>
        Kind switch
        {
            DuplicateIdentifierKind.Phone => new RegisterPatientCommand(
                PatientAuthProvider.Phone,
                SecondFullName,
                Identifier,
                null,
                PatientRegistrationTestHost.ValidPassword,
                null),
            DuplicateIdentifierKind.Email => new RegisterPatientCommand(
                PatientAuthProvider.Email,
                SecondFullName,
                null,
                Identifier,
                PatientRegistrationTestHost.ValidPassword,
                null),
            _ => throw new InvalidOperationException("Unsupported duplicate identifier kind.")
        };
}

public static class PatientRegistrationArbitraries
{
    private static readonly string[] SampleNames =
    [
        "Jane Doe",
        "John Smith",
        "Tariro Moyo",
        "Chipo Ncube",
        "David Okonkwo"
    ];

    public static Arbitrary<ValidPatientRegistration> ValidPatientRegistration() =>
        Gen.OneOf(
            PhoneRegistration(),
            EmailRegistration(),
            GoogleRegistration(),
            AppleRegistration())
            .ToArbitrary();

    public static Arbitrary<DuplicateRegistrationCase> DuplicateRegistrationCase() =>
        Gen.OneOf(PhoneDuplicateCase(), EmailDuplicateCase()).ToArbitrary();

    private static Gen<ValidPatientRegistration> PhoneRegistration() =>
        from name in Gen.Elements(SampleNames)
        from suffix in Gen.Choose(10_000_000, 99_999_999)
        select new ValidPatientRegistration(
            PatientAuthProvider.Phone,
            name,
            $"+2637{suffix}",
            null,
            PatientRegistrationTestHost.ValidPassword,
            null);

    private static Gen<ValidPatientRegistration> EmailRegistration() =>
        from name in Gen.Elements(SampleNames)
        from unique in Arb.Default.Guid().Generator.Where(g => g != Guid.Empty)
        select new ValidPatientRegistration(
            PatientAuthProvider.Email,
            name,
            null,
            $"patient-{unique:N}@example.com",
            PatientRegistrationTestHost.ValidPassword,
            null);

    private static Gen<ValidPatientRegistration> GoogleRegistration() =>
        from name in Gen.Elements(SampleNames)
        from unique in Arb.Default.Guid().Generator.Where(g => g != Guid.Empty)
        let email = $"google-{unique:N}@example.com"
        let token = PatientRegistrationTestHost.CreateSocialIdToken(
            $"google-{unique:N}",
            email,
            name)
        select new ValidPatientRegistration(
            PatientAuthProvider.Google,
            name,
            null,
            null,
            null,
            token);

    private static Gen<ValidPatientRegistration> AppleRegistration() =>
        from name in Gen.Elements(SampleNames)
        from unique in Arb.Default.Guid().Generator.Where(g => g != Guid.Empty)
        let email = $"apple-{unique:N}@example.com"
        let token = PatientRegistrationTestHost.CreateSocialIdToken(
            $"apple-{unique:N}",
            email,
            name)
        select new ValidPatientRegistration(
            PatientAuthProvider.Apple,
            name,
            null,
            null,
            null,
            token);

    private static Gen<DuplicateRegistrationCase> PhoneDuplicateCase() =>
        from firstName in Gen.Elements(SampleNames)
        from secondName in Gen.Elements(SampleNames)
        from suffix in Gen.Choose(10_000_000, 99_999_999)
        select new DuplicateRegistrationCase(
            DuplicateIdentifierKind.Phone,
            firstName,
            secondName,
            $"+2637{suffix}");

    private static Gen<DuplicateRegistrationCase> EmailDuplicateCase() =>
        from firstName in Gen.Elements(SampleNames)
        from secondName in Gen.Elements(SampleNames)
        from unique in Arb.Default.Guid().Generator.Where(g => g != Guid.Empty)
        select new DuplicateRegistrationCase(
            DuplicateIdentifierKind.Email,
            firstName,
            secondName,
            $"duplicate-{unique:N}@example.com");
}
