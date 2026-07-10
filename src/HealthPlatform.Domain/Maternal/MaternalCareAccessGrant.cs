using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.Maternal;

public sealed class MaternalCareAccessGrant : Entity
{
    private MaternalCareAccessGrant()
    {
    }

    public Guid AntenatalRecordId { get; private set; }

    public Guid PatientId { get; private set; }

    public Guid DoctorId { get; private set; }

    public bool ShareAntenatalRecord { get; private set; }

    public bool ShareBirthPlan { get; private set; }

    public DateTime GrantedAtUtc { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }

    public bool IsActive => RevokedAtUtc is null;

    public static MaternalCareAccessGrant Grant(
        Guid antenatalRecordId,
        Guid patientId,
        Guid doctorId,
        bool shareAntenatalRecord,
        bool shareBirthPlan,
        DateTime grantedAtUtc)
    {
        if (antenatalRecordId == Guid.Empty)
        {
            throw new ArgumentException("Antenatal record id is required.", nameof(antenatalRecordId));
        }

        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (doctorId == Guid.Empty)
        {
            throw new ArgumentException("Doctor id is required.", nameof(doctorId));
        }

        if (!shareAntenatalRecord && !shareBirthPlan)
        {
            throw new ArgumentException(
                "At least one of antenatal record or birth plan sharing must be enabled.",
                nameof(shareAntenatalRecord));
        }

        if (grantedAtUtc == default || grantedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Granted timestamp must be UTC.", nameof(grantedAtUtc));
        }

        return new MaternalCareAccessGrant
        {
            Id = Guid.CreateVersion7(),
            AntenatalRecordId = antenatalRecordId,
            PatientId = patientId,
            DoctorId = doctorId,
            ShareAntenatalRecord = shareAntenatalRecord,
            ShareBirthPlan = shareBirthPlan,
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

    public void Reactivate(
        bool shareAntenatalRecord,
        bool shareBirthPlan,
        DateTime grantedAtUtc)
    {
        if (!shareAntenatalRecord && !shareBirthPlan)
        {
            throw new ArgumentException(
                "At least one of antenatal record or birth plan sharing must be enabled.",
                nameof(shareAntenatalRecord));
        }

        if (grantedAtUtc == default || grantedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Granted timestamp must be UTC.", nameof(grantedAtUtc));
        }

        ShareAntenatalRecord = shareAntenatalRecord;
        ShareBirthPlan = shareBirthPlan;
        GrantedAtUtc = grantedAtUtc;
        RevokedAtUtc = null;
        Touch();
    }
}
