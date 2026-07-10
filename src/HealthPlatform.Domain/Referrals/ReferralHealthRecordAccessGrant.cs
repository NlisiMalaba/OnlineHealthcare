using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.Referrals;

public sealed class ReferralHealthRecordAccessGrant : Entity
{
    private ReferralHealthRecordAccessGrant()
    {
        SharedHealthRecordSections = [];
    }

    public Guid ReferralId { get; private set; }

    public Guid PatientId { get; private set; }

    public Guid DoctorId { get; private set; }

    public IReadOnlyList<string> SharedHealthRecordSections { get; private set; }

    public DateTime GrantedAtUtc { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }

    public static ReferralHealthRecordAccessGrant Create(
        Guid referralId,
        Guid patientId,
        Guid doctorId,
        IReadOnlyCollection<string> sharedHealthRecordSections,
        DateTime grantedAtUtc)
    {
        if (referralId == Guid.Empty)
        {
            throw new ArgumentException("Referral id is required.", nameof(referralId));
        }

        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (doctorId == Guid.Empty)
        {
            throw new ArgumentException("Doctor id is required.", nameof(doctorId));
        }

        if (grantedAtUtc == default || grantedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Granted timestamp must be UTC.", nameof(grantedAtUtc));
        }

        var sections = sharedHealthRecordSections
            .Select(section => section.Trim())
            .Where(section => section.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (sections.Length == 0)
        {
            throw new ArgumentException(
                "At least one shared section is required.",
                nameof(sharedHealthRecordSections));
        }

        return new ReferralHealthRecordAccessGrant
        {
            Id = Guid.CreateVersion7(),
            ReferralId = referralId,
            PatientId = patientId,
            DoctorId = doctorId,
            SharedHealthRecordSections = sections,
            GrantedAtUtc = grantedAtUtc,
            CreatedAtUtc = grantedAtUtc,
            UpdatedAtUtc = grantedAtUtc
        };
    }

    public void Revoke(DateTime revokedAtUtc)
    {
        if (revokedAtUtc == default || revokedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Revoked timestamp must be UTC.", nameof(revokedAtUtc));
        }

        if (RevokedAtUtc.HasValue)
        {
            return;
        }

        RevokedAtUtc = revokedAtUtc;
        Touch();
    }
}
