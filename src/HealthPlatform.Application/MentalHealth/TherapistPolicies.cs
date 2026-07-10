namespace HealthPlatform.Application.MentalHealth;

public static class TherapistPolicies
{
    private static readonly string[] LicensedTherapistSpecialties =
    [
        "psychiatry",
        "psychology",
        "clinical psychology",
        "mental health",
        "psychotherapy",
        "therapist",
        "counselling",
        "counseling"
    ];

    public static bool IsLicensedTherapist(string specialty) =>
        !string.IsNullOrWhiteSpace(specialty)
        && LicensedTherapistSpecialties.Any(
            licensed => specialty.Contains(licensed, StringComparison.OrdinalIgnoreCase));
}
