using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.Insurance;

public sealed class PatientInsurancePolicy : Entity
{
    private PatientInsurancePolicy()
    {
        InsurerCode = string.Empty;
        PolicyNumber = string.Empty;
    }

    public Guid PatientId { get; private set; }

    public string InsurerCode { get; private set; }

    public string PolicyNumber { get; private set; }

    public string? MemberNumber { get; private set; }

    public DateOnly ValidFrom { get; private set; }

    public DateOnly? ValidTo { get; private set; }

    public bool IsActive { get; private set; }

    public static PatientInsurancePolicy Create(
        Guid patientId,
        string insurerCode,
        string policyNumber,
        string? memberNumber,
        DateOnly validFrom,
        DateOnly? validTo)
    {
        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(insurerCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(policyNumber);

        if (validTo is not null && validTo < validFrom)
        {
            throw new ArgumentException("Valid to date cannot be before valid from date.", nameof(validTo));
        }

        return new PatientInsurancePolicy
        {
            PatientId = patientId,
            InsurerCode = insurerCode.Trim().ToLowerInvariant(),
            PolicyNumber = policyNumber.Trim(),
            MemberNumber = string.IsNullOrWhiteSpace(memberNumber) ? null : memberNumber.Trim(),
            ValidFrom = validFrom,
            ValidTo = validTo,
            IsActive = true
        };
    }

    public bool IsActiveOn(DateOnly date) =>
        IsActive
        && date >= ValidFrom
        && (ValidTo is null || date <= ValidTo);

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }
}
