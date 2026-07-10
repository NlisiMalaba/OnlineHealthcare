namespace HealthPlatform.Application.Maternal;

public static class ObstetricPolicies
{
    private static readonly string[] LicensedObstetricSpecialties =
    [
        "obstetrics",
        "obstetric",
        "gynecology",
        "gynaecology",
        "maternal-fetal",
        "maternal fetal",
        "ob-gyn",
        "obgyn"
    ];

    public static bool IsLicensedObstetrician(string specialty) =>
        !string.IsNullOrWhiteSpace(specialty)
        && LicensedObstetricSpecialties.Any(
            licensed => specialty.Contains(licensed, StringComparison.OrdinalIgnoreCase));
}
