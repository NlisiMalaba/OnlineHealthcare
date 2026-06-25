using FsCheck.Xunit;
using HealthPlatform.Domain.Prescriptions;
using HealthPlatform.Tests.Arbitraries;

namespace HealthPlatform.Tests.Properties;

public sealed class PrescriptionDefaultExpiryPropertyTests
{
    // Feature: online-healthcare-platform, Property 12: Prescription Default Expiry
    [Property(Arbitrary = [typeof(PrescriptionArbitraries)], MaxTest = 100)]
    public bool Prescription_without_explicit_expiry_defaults_to_issued_at_plus_30_days(
        PrescriptionDefaultExpiryCase input)
    {
        var prescription = Prescription.Issue(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Amoxicillin",
            "500mg",
            "Twice daily",
            input.DurationDays,
            null,
            null,
            null,
            input.IssuedAtUtc);

        return prescription.ExpiresAtUtc
            == input.IssuedAtUtc.AddDays(PrescriptionPolicies.DefaultExpiryDays);
    }
}
