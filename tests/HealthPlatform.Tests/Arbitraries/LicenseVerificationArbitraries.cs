using FsCheck;
using HealthPlatform.Tests.Arbitraries;

namespace HealthPlatform.Tests.Arbitraries;

public sealed record LicenseVerificationTransitionCase(
    ValidDoctorRegistration Registration,
    bool VerifyLicense,
    string RejectionReason);

public static class LicenseVerificationArbitraries
{
    private static readonly string[] RejectionReasonPrefixes =
    [
        "License number could not be validated with HPCZ registry.",
        "Submitted credentials did not match the license number.",
        "Medical license is expired.",
        "License is registered to a different practitioner."
    ];

    public static Arbitrary<LicenseVerificationTransitionCase> LicenseVerificationTransitionCase() =>
        (from registration in DoctorRegistrationArbitraries.ValidDoctorRegistration().Generator
         from verify in Gen.OneOf(Gen.Constant(true), Gen.Constant(false))
         from reason in verify ? Gen.Constant(string.Empty) : ValidRejectionReason()
         select new LicenseVerificationTransitionCase(registration, verify, reason))
        .ToArbitrary();

    private static Gen<string> ValidRejectionReason() =>
        from prefix in Gen.Elements(RejectionReasonPrefixes)
        from suffix in Gen.Choose(1, 99_999)
        select $"{prefix} Ref #{suffix}.";
}
