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

    [Fact]
    public void MarkDispensed_transitions_active_prescription_to_dispensed()
    {
        var issuedAtUtc = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);
        var prescription = CreateActivePrescription(issuedAtUtc);

        prescription.MarkDispensed(issuedAtUtc.AddDays(1));

        Assert.Equal(PrescriptionStatus.Dispensed, prescription.Status);
    }

    [Fact]
    public void MarkDispensed_rejects_already_dispensed_prescription()
    {
        var issuedAtUtc = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);
        var prescription = CreateActivePrescription(issuedAtUtc);
        prescription.MarkDispensed(issuedAtUtc.AddDays(1));

        Assert.Throws<PrescriptionDispensedException>(() =>
            prescription.MarkDispensed(issuedAtUtc.AddDays(2)));
    }

    [Fact]
    public void MarkDispensed_rejects_expired_prescription()
    {
        var issuedAtUtc = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var prescription = CreateActivePrescription(issuedAtUtc);

        Assert.Throws<PrescriptionExpiredException>(() =>
            prescription.MarkDispensed(issuedAtUtc.AddDays(PrescriptionPolicies.DefaultExpiryDays)));
    }

    private static Prescription CreateActivePrescription(DateTime issuedAtUtc) =>
        Prescription.Issue(
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

    [Fact]
    public void Cancel_records_mandatory_reason_and_sets_status()
    {
        var issuedAtUtc = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);
        var prescription = CreateActivePrescription(issuedAtUtc);

        prescription.Cancel("Treatment plan changed", issuedAtUtc.AddHours(1));

        Assert.Equal(PrescriptionStatus.Cancelled, prescription.Status);
        Assert.Equal("Treatment plan changed", prescription.CancellationReason);
    }

    [Fact]
    public void Cancel_rejects_dispensed_prescription()
    {
        var issuedAtUtc = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);
        var prescription = CreateActivePrescription(issuedAtUtc);
        prescription.MarkDispensed(issuedAtUtc.AddDays(1));

        Assert.Throws<PrescriptionNotCancellableException>(() =>
            prescription.Cancel("Too late", issuedAtUtc.AddDays(2)));
    }
}
