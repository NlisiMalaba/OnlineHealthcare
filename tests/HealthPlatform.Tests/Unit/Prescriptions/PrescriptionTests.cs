using HealthPlatform.Domain.Prescriptions;
using Xunit;

namespace HealthPlatform.Tests.Unit.Prescriptions;

public sealed class PrescriptionTests
{
    [Fact]
    public void ApplyDefaultExpiryIfUnset_sets_expiry_to_issued_at_plus_30_days()
    {
        var issuedAtUtc = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);
        var prescription = Prescription.Issue(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Amoxicillin",
            "500mg",
            "Twice daily",
            7,
            null,
            null,
            null,
            issuedAtUtc);

        Assert.Equal(issuedAtUtc.AddDays(PrescriptionPolicies.DefaultExpiryDays), prescription.ExpiresAtUtc);
    }

    [Fact]
    public void Issue_preserves_explicit_expiry_when_provided()
    {
        var issuedAtUtc = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);
        var expiresAtUtc = issuedAtUtc.AddDays(14);

        var prescription = Prescription.Issue(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Amoxicillin",
            "500mg",
            "Twice daily",
            7,
            "Take with food",
            expiresAtUtc,
            null,
            issuedAtUtc);

        Assert.Equal(expiresAtUtc, prescription.ExpiresAtUtc);
        Assert.Equal("active", prescription.Status.ToString().ToLowerInvariant());
    }
}
