using HealthPlatform.Domain.Common;
using HealthPlatform.Domain.Referrals.Events;

namespace HealthPlatform.Domain.Referrals;

public sealed class Referral : Entity
{
    private Referral()
    {
        Reason = string.Empty;
        SharedHealthRecordSections = [];
    }

    public Guid PatientId { get; private set; }

    public Guid ReferringDoctorId { get; private set; }

    public Guid? ReceivingDoctorId { get; private set; }

    public string? ReceivingHospitalName { get; private set; }

    public string Reason { get; private set; }

    public string? ClinicalNotes { get; private set; }

    public IReadOnlyList<string> SharedHealthRecordSections { get; private set; }

    public DateTime PatientConsentAtUtc { get; private set; }

    public ReferralStatus Status { get; private set; }

    public static Referral Create(
        Guid patientId,
        Guid referringDoctorId,
        Guid? receivingDoctorId,
        string? receivingHospitalName,
        string reason,
        string? clinicalNotes,
        IReadOnlyCollection<string> sharedHealthRecordSections,
        DateTime patientConsentAtUtc,
        DateTime createdAtUtc)
    {
        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (referringDoctorId == Guid.Empty)
        {
            throw new ArgumentException("Referring doctor id is required.", nameof(referringDoctorId));
        }

        if (!receivingDoctorId.HasValue && string.IsNullOrWhiteSpace(receivingHospitalName))
        {
            throw new ArgumentException(
                "Either receiving doctor or receiving hospital is required.",
                nameof(receivingDoctorId));
        }

        if (receivingDoctorId.HasValue && receivingDoctorId.Value == Guid.Empty)
        {
            throw new ArgumentException("Receiving doctor id is invalid.", nameof(receivingDoctorId));
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Referral reason is required.", nameof(reason));
        }

        if (sharedHealthRecordSections.Count == 0)
        {
            throw new ArgumentException(
                "At least one health record section must be shared.",
                nameof(sharedHealthRecordSections));
        }

        if (patientConsentAtUtc == default || patientConsentAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Patient consent timestamp must be UTC.", nameof(patientConsentAtUtc));
        }

        if (createdAtUtc == default || createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Creation timestamp must be UTC.", nameof(createdAtUtc));
        }

        var sections = sharedHealthRecordSections
            .Select(section => section.Trim())
            .Where(section => section.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (sections.Length == 0)
        {
            throw new ArgumentException(
                "At least one valid health record section must be shared.",
                nameof(sharedHealthRecordSections));
        }

        var referral = new Referral
        {
            Id = Guid.CreateVersion7(),
            PatientId = patientId,
            ReferringDoctorId = referringDoctorId,
            ReceivingDoctorId = receivingDoctorId,
            ReceivingHospitalName = string.IsNullOrWhiteSpace(receivingHospitalName)
                ? null
                : receivingHospitalName.Trim(),
            Reason = reason.Trim(),
            ClinicalNotes = string.IsNullOrWhiteSpace(clinicalNotes) ? null : clinicalNotes.Trim(),
            SharedHealthRecordSections = sections,
            PatientConsentAtUtc = patientConsentAtUtc,
            Status = ReferralStatus.Pending,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc
        };

        referral.RaiseDomainEvent(new ReferralCreatedDomainEvent(
            referral.Id,
            referral.PatientId,
            referral.ReferringDoctorId,
            referral.ReceivingDoctorId,
            referral.ReceivingHospitalName,
            referral.Reason,
            referral.PatientConsentAtUtc,
            createdAtUtc));

        return referral;
    }
}
