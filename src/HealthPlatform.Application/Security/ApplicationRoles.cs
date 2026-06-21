namespace HealthPlatform.Application.Security;

/// <summary>
/// ASP.NET Core Identity role names and authorization policy names (never use raw literals in handlers).
/// </summary>
public static class ApplicationRoles
{
    public const string Patient = "patient";
    public const string Doctor = "doctor";
    public const string Pharmacy = "pharmacy";
    public const string LabPartner = "lab_partner";
    public const string Insurer = "insurer";
    public const string Admin = "admin";

    public static IReadOnlyList<string> All { get; } =
    [
        Patient,
        Doctor,
        Pharmacy,
        LabPartner,
        Insurer,
        Admin
    ];

    /// <summary>
    /// Roles that must enroll and use MFA (TOTP and/or SMS OTP) per Requirements 17.3.
    /// </summary>
    public static IReadOnlyList<string> MandatoryTwoFactorRoles { get; } = [Doctor, Pharmacy, Admin];

    public static bool IsMandatoryTwoFactorRole(string roleName) =>
        MandatoryTwoFactorRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
}
