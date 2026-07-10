namespace HealthPlatform.Domain.MentalHealth;

public sealed class MoodChartSharingConsent : Common.Entity
{
    private MoodChartSharingConsent()
    {
    }

    public Guid PatientId { get; private set; }

    public Guid TherapistId { get; private set; }

    public DateTime GrantedAtUtc { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }

    public bool IsActive => RevokedAtUtc is null;

    public static MoodChartSharingConsent Grant(
        Guid patientId,
        Guid therapistId,
        DateTime grantedAtUtc)
    {
        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (therapistId == Guid.Empty)
        {
            throw new ArgumentException("Therapist id is required.", nameof(therapistId));
        }

        if (grantedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Grant time must be UTC.", nameof(grantedAtUtc));
        }

        return new MoodChartSharingConsent
        {
            Id = Guid.CreateVersion7(),
            PatientId = patientId,
            TherapistId = therapistId,
            GrantedAtUtc = grantedAtUtc
        };
    }

    public void Revoke(DateTime revokedAtUtc)
    {
        if (RevokedAtUtc.HasValue)
        {
            return;
        }

        if (revokedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Revoke time must be UTC.", nameof(revokedAtUtc));
        }

        RevokedAtUtc = revokedAtUtc;
        Touch();
    }

    public void Reactivate(DateTime grantedAtUtc)
    {
        if (grantedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Grant time must be UTC.", nameof(grantedAtUtc));
        }

        GrantedAtUtc = grantedAtUtc;
        RevokedAtUtc = null;
        Touch();
    }
}
