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

    public DateTime? RespondedAtUtc { get; private set; }

    public string? ResponseReason { get; private set; }

    public string? ConsultationSummaryEntryId { get; private set; }

    public DateTime? TimeoutReminderSentAtUtc { get; private set; }

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

    public void Accept(DateTime respondedAtUtc)
    {
        EnsurePendingForResponse();
        EnsureUtcTimestamp(respondedAtUtc, nameof(respondedAtUtc));

        Status = ReferralStatus.Accepted;
        RespondedAtUtc = respondedAtUtc;
        ResponseReason = null;
        Touch();

        RaiseDomainEvent(new ReferralStatusChangedDomainEvent(
            Id,
            PatientId,
            ReferringDoctorId,
            ReceivingDoctorId,
            Status,
            null,
            respondedAtUtc));
    }

    public void Decline(string reason, DateTime respondedAtUtc)
    {
        EnsurePendingForResponse();
        EnsureUtcTimestamp(respondedAtUtc, nameof(respondedAtUtc));

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Decline reason is required.", nameof(reason));
        }

        Status = ReferralStatus.Declined;
        RespondedAtUtc = respondedAtUtc;
        ResponseReason = reason.Trim();
        Touch();

        RaiseDomainEvent(new ReferralStatusChangedDomainEvent(
            Id,
            PatientId,
            ReferringDoctorId,
            ReceivingDoctorId,
            Status,
            ResponseReason,
            respondedAtUtc));
    }

    public void RequestAdditionalInformation(string message, DateTime respondedAtUtc)
    {
        EnsurePendingForResponse();
        EnsureUtcTimestamp(respondedAtUtc, nameof(respondedAtUtc));

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Additional information request message is required.", nameof(message));
        }

        Status = ReferralStatus.NeedsAdditionalInformation;
        RespondedAtUtc = respondedAtUtc;
        ResponseReason = message.Trim();
        Touch();

        RaiseDomainEvent(new ReferralStatusChangedDomainEvent(
            Id,
            PatientId,
            ReferringDoctorId,
            ReceivingDoctorId,
            Status,
            ResponseReason,
            respondedAtUtc));
    }

    private void EnsurePendingForResponse()
    {
        if (Status is not (ReferralStatus.Pending or ReferralStatus.NeedsAdditionalInformation))
        {
            throw new ReferralResponseNotAllowedException(Id, Status);
        }
    }

    private static void EnsureUtcTimestamp(DateTime value, string parameterName)
    {
        if (value == default || value.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Timestamp must be UTC.", parameterName);
        }
    }

    public void Complete(string consultationSummaryEntryId, DateTime completedAtUtc)
    {
        if (Status != ReferralStatus.Accepted)
        {
            throw new ReferralCompletionNotAllowedException(Id, Status);
        }

        if (string.IsNullOrWhiteSpace(consultationSummaryEntryId))
        {
            throw new ArgumentException("Consultation summary entry id is required.", nameof(consultationSummaryEntryId));
        }

        EnsureUtcTimestamp(completedAtUtc, nameof(completedAtUtc));

        ConsultationSummaryEntryId = consultationSummaryEntryId.Trim();
        Status = ReferralStatus.Completed;
        RespondedAtUtc = completedAtUtc;
        ResponseReason = null;
        Touch();

        RaiseDomainEvent(new ReferralStatusChangedDomainEvent(
            Id,
            PatientId,
            ReferringDoctorId,
            ReceivingDoctorId,
            Status,
            null,
            completedAtUtc));
    }

    public bool MarkTimeoutReminderSent(DateTime sentAtUtc)
    {
        EnsureUtcTimestamp(sentAtUtc, nameof(sentAtUtc));
        if (Status != ReferralStatus.Pending || TimeoutReminderSentAtUtc.HasValue)
        {
            return false;
        }

        TimeoutReminderSentAtUtc = sentAtUtc;
        Touch();
        return true;
    }
}
