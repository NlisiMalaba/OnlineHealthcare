namespace HealthPlatform.Application.Security;

/// <summary>
/// ASP.NET Core policy names (1:1 with primary roles for coarse RBAC).
/// </summary>
public static class AuthorizationPolicies
{
    public const string Patient = ApplicationRoles.Patient;
    public const string Doctor = ApplicationRoles.Doctor;
    public const string Pharmacy = ApplicationRoles.Pharmacy;
    public const string LabPartner = ApplicationRoles.LabPartner;
    public const string Insurer = ApplicationRoles.Insurer;
    public const string Admin = ApplicationRoles.Admin;
}
