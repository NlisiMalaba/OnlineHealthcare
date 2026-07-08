using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.Labs;

public sealed class LabOrder : Entity
{
    private LabOrder()
    {
        LabPartnerCode = string.Empty;
        TestCode = string.Empty;
    }

    public Guid PatientId { get; private set; }

    public Guid HealthRecordId { get; private set; }

    public Guid? OrderingDoctorId { get; private set; }

    public LabOrderRequestSource RequestSource { get; private set; }

    public LabOrderStatus Status { get; private set; }

    public string LabPartnerCode { get; private set; }

    public string TestCode { get; private set; }

    public string? ClinicalNotes { get; private set; }

    public string? LabPartnerOrderReference { get; private set; }

    public DateTime? ApprovedAtUtc { get; private set; }

    public static LabOrder CreateDoctorOrdered(
        Guid patientId,
        Guid healthRecordId,
        Guid orderingDoctorId,
        string labPartnerCode,
        string testCode,
        string? clinicalNotes,
        DateTime createdAtUtc)
    {
        EnsureIdentity(patientId, nameof(patientId));
        EnsureIdentity(healthRecordId, nameof(healthRecordId));
        EnsureIdentity(orderingDoctorId, nameof(orderingDoctorId));

        return new LabOrder
        {
            PatientId = patientId,
            HealthRecordId = healthRecordId,
            OrderingDoctorId = orderingDoctorId,
            RequestSource = LabOrderRequestSource.DoctorOrdered,
            Status = LabOrderStatus.SubmittedToLabPartner,
            LabPartnerCode = NormalizeCode(labPartnerCode, nameof(labPartnerCode)),
            TestCode = NormalizeCode(testCode, nameof(testCode)),
            ClinicalNotes = NormalizeNotes(clinicalNotes),
            CreatedAtUtc = createdAtUtc
        };
    }

    public static LabOrder CreatePatientRequested(
        Guid patientId,
        Guid healthRecordId,
        string labPartnerCode,
        string testCode,
        string? clinicalNotes,
        DateTime createdAtUtc)
    {
        EnsureIdentity(patientId, nameof(patientId));
        EnsureIdentity(healthRecordId, nameof(healthRecordId));

        return new LabOrder
        {
            PatientId = patientId,
            HealthRecordId = healthRecordId,
            RequestSource = LabOrderRequestSource.PatientRequested,
            Status = LabOrderStatus.PendingDoctorApproval,
            LabPartnerCode = NormalizeCode(labPartnerCode, nameof(labPartnerCode)),
            TestCode = NormalizeCode(testCode, nameof(testCode)),
            ClinicalNotes = NormalizeNotes(clinicalNotes),
            CreatedAtUtc = createdAtUtc
        };
    }

    public void MarkSubmitted(string partnerReference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partnerReference);
        LabPartnerOrderReference = partnerReference.Trim();
        Status = LabOrderStatus.SubmittedToLabPartner;
        Touch();
    }

    public void Approve(Guid doctorId, DateTime approvedAtUtc)
    {
        EnsureIdentity(doctorId, nameof(doctorId));
        if (RequestSource != LabOrderRequestSource.PatientRequested || Status != LabOrderStatus.PendingDoctorApproval)
        {
            throw new LabOrderApprovalNotAllowedException(RequestSource, Status);
        }

        OrderingDoctorId = doctorId;
        ApprovedAtUtc = approvedAtUtc;
        Touch();
    }

    private static void EnsureIdentity(Guid value, string name)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Identifier is required.", name);
        }
    }

    private static string NormalizeCode(string code, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return code.Trim().ToUpperInvariant();
    }

    private static string? NormalizeNotes(string? notes) =>
        string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
}
