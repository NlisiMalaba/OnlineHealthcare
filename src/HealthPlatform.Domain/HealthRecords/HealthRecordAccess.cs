using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.HealthRecords;

public sealed class HealthRecordAccess : Entity
{
    private HealthRecordAccess()
    {
        Sections = [];
    }

    public Guid HealthRecordId { get; private set; }

    public Guid GrantedToDoctorId { get; private set; }

    public DateTime GrantedAtUtc { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }

    public HealthRecordAccessType AccessType { get; private set; }

    public List<string> Sections { get; private set; } = [];

    public bool IsActive => RevokedAtUtc is null;

    public static HealthRecordAccess Grant(
        Guid healthRecordId,
        Guid doctorId,
        HealthRecordAccessType accessType,
        IReadOnlyList<string>? sections,
        DateTime grantedAtUtc)
    {
        if (healthRecordId == Guid.Empty)
        {
            throw new ArgumentException("Health record id is required.", nameof(healthRecordId));
        }

        if (doctorId == Guid.Empty)
        {
            throw new ArgumentException("Doctor id is required.", nameof(doctorId));
        }

        if (accessType == HealthRecordAccessType.Sections && (sections is null || sections.Count == 0))
        {
            throw new ArgumentException("Sections are required for section-scoped access.", nameof(sections));
        }

        return new HealthRecordAccess
        {
            Id = Guid.CreateVersion7(),
            HealthRecordId = healthRecordId,
            GrantedToDoctorId = doctorId,
            GrantedAtUtc = grantedAtUtc,
            AccessType = accessType,
            Sections = accessType == HealthRecordAccessType.Sections
                ? sections!.Select(section => section.Trim()).Where(section => section.Length > 0).ToList()
                : []
        };
    }

    public void Revoke(DateTime revokedAtUtc)
    {
        if (RevokedAtUtc is not null)
        {
            throw new InvalidOperationException("Health record access has already been revoked.");
        }

        RevokedAtUtc = revokedAtUtc;
        Touch();
    }

    public void Reactivate(DateTime grantedAtUtc, HealthRecordAccessType accessType, IReadOnlyList<string>? sections)
    {
        if (IsActive)
        {
            throw new InvalidOperationException("Health record access is already active.");
        }

        if (accessType == HealthRecordAccessType.Sections && (sections is null || sections.Count == 0))
        {
            throw new ArgumentException("Sections are required for section-scoped access.", nameof(sections));
        }

        GrantedAtUtc = grantedAtUtc;
        RevokedAtUtc = null;
        AccessType = accessType;
        Sections = accessType == HealthRecordAccessType.Sections
            ? sections!.Select(section => section.Trim()).Where(section => section.Length > 0).ToList()
            : [];
        Touch();
    }
}
