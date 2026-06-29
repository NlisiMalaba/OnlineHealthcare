using HealthPlatform.Domain.Common;
using HealthPlatform.Domain.Insurance.Events;

namespace HealthPlatform.Domain.Insurance;

public sealed class InsuranceClaim : Entity
{
    private InsuranceClaim()
    {
        InsurerCode = string.Empty;
        Currency = string.Empty;
    }

    public Guid PatientId { get; private set; }

    public Guid PatientInsurancePolicyId { get; private set; }

    public string InsurerCode { get; private set; }

    public InsuranceClaimType ClaimType { get; private set; }

    public Guid? AppointmentId { get; private set; }

    public Guid? MedicationOrderId { get; private set; }

    public Guid? LabOrderId { get; private set; }

    public long AmountMinorUnits { get; private set; }

    public string Currency { get; private set; }

    public InsuranceClaimStatus Status { get; private set; }

    public string? InsurerClaimReference { get; private set; }

    public string? StatusReason { get; private set; }

    public DateTime? SubmittedAtUtc { get; private set; }

    public DateTime? LastStatusCheckedAtUtc { get; private set; }

    public static InsuranceClaim Create(
        Guid patientId,
        Guid patientInsurancePolicyId,
        string insurerCode,
        InsuranceClaimType claimType,
        long amountMinorUnits,
        string currency,
        Guid? appointmentId,
        Guid? medicationOrderId,
        Guid? labOrderId)
    {
        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (patientInsurancePolicyId == Guid.Empty)
        {
            throw new ArgumentException("Insurance policy id is required.", nameof(patientInsurancePolicyId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(insurerCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        if (amountMinorUnits <= 0)
        {
            throw new ArgumentException("Amount must be positive.", nameof(amountMinorUnits));
        }

        ValidateClaimTarget(claimType, appointmentId, medicationOrderId, labOrderId);

        return new InsuranceClaim
        {
            PatientId = patientId,
            PatientInsurancePolicyId = patientInsurancePolicyId,
            InsurerCode = insurerCode.Trim().ToLowerInvariant(),
            ClaimType = claimType,
            AppointmentId = appointmentId,
            MedicationOrderId = medicationOrderId,
            LabOrderId = labOrderId,
            AmountMinorUnits = amountMinorUnits,
            Currency = currency.Trim().ToUpperInvariant(),
            Status = InsuranceClaimStatus.Draft
        };
    }

    public void MarkSubmitted(string insurerClaimReference, DateTime submittedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(insurerClaimReference);

        InsurerClaimReference = insurerClaimReference.Trim();
        SubmittedAtUtc = submittedAtUtc;
        Status = InsuranceClaimStatus.Submitted;
        Touch();
        RaiseDomainEvent(new InsuranceClaimSubmittedDomainEvent(Id, PatientId, InsurerCode));
    }

    public bool TryUpdateStatus(InsuranceClaimStatus newStatus, string? statusReason, DateTime occurredAtUtc)
    {
        if (newStatus == Status)
        {
            LastStatusCheckedAtUtc = occurredAtUtc;
            Touch();
            return false;
        }

        if (!IsValidTransition(Status, newStatus))
        {
            return false;
        }

        var previousStatus = Status;
        Status = newStatus;
        StatusReason = string.IsNullOrWhiteSpace(statusReason) ? null : statusReason.Trim();
        LastStatusCheckedAtUtc = occurredAtUtc;
        Touch();
        RaiseDomainEvent(new InsuranceClaimStatusChangedDomainEvent(
            Id,
            PatientId,
            previousStatus,
            newStatus,
            StatusReason));
        return true;
    }

    public void RecordStatusCheck(DateTime checkedAtUtc)
    {
        LastStatusCheckedAtUtc = checkedAtUtc;
        Touch();
    }

    private static void ValidateClaimTarget(
        InsuranceClaimType claimType,
        Guid? appointmentId,
        Guid? medicationOrderId,
        Guid? labOrderId)
    {
        var targetCount = new[] { appointmentId, medicationOrderId, labOrderId }.Count(id => id is not null);
        if (targetCount != 1)
        {
            throw new ArgumentException("Exactly one claim target id is required.");
        }

        var matchesType = claimType switch
        {
            InsuranceClaimType.Consultation => appointmentId is not null,
            InsuranceClaimType.Medication => medicationOrderId is not null,
            InsuranceClaimType.LabTest => labOrderId is not null,
            _ => false
        };

        if (!matchesType)
        {
            throw new ArgumentException("Claim type does not match the provided target id.");
        }
    }

    private static bool IsValidTransition(InsuranceClaimStatus current, InsuranceClaimStatus next) =>
        (current, next) switch
        {
            (InsuranceClaimStatus.Draft, InsuranceClaimStatus.Submitted) => true,
            (InsuranceClaimStatus.Submitted, InsuranceClaimStatus.Processing) => true,
            (InsuranceClaimStatus.Submitted, InsuranceClaimStatus.Approved) => true,
            (InsuranceClaimStatus.Submitted, InsuranceClaimStatus.Rejected) => true,
            (InsuranceClaimStatus.Processing, InsuranceClaimStatus.Approved) => true,
            (InsuranceClaimStatus.Processing, InsuranceClaimStatus.Rejected) => true,
            (InsuranceClaimStatus.Processing, InsuranceClaimStatus.Paid) => true,
            (InsuranceClaimStatus.Approved, InsuranceClaimStatus.Paid) => true,
            (InsuranceClaimStatus.Approved, InsuranceClaimStatus.Rejected) => true,
            _ => false
        };
}
