using HealthPlatform.Domain.Common;
using HealthPlatform.Domain.Prescriptions.Events;

namespace HealthPlatform.Domain.Prescriptions;

public sealed class Prescription : Entity
{
    private Prescription()
    {
        MedicationName = string.Empty;
        Dosage = string.Empty;
        Frequency = string.Empty;
    }

    public Guid DoctorId { get; private set; }

    public Guid PatientId { get; private set; }

    public Guid HealthRecordId { get; private set; }

    public Guid? AppointmentId { get; private set; }

    public string MedicationName { get; private set; }

    public string Dosage { get; private set; }

    public string Frequency { get; private set; }

    public int DurationDays { get; private set; }

    public string? SpecialInstructions { get; private set; }

    public PrescriptionStatus Status { get; private set; }

    public DateTime IssuedAtUtc { get; private set; }

    public DateTime ExpiresAtUtc { get; private set; }

    public string? CancellationReason { get; private set; }

    public static Prescription Issue(
        Guid doctorId,
        Guid patientId,
        Guid healthRecordId,
        string medicationName,
        string dosage,
        string frequency,
        int durationDays,
        string? specialInstructions,
        DateTime? expiresAtUtc,
        Guid? appointmentId,
        DateTime issuedAtUtc)
    {
        if (doctorId == Guid.Empty)
        {
            throw new ArgumentException("Doctor id is required.", nameof(doctorId));
        }

        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (healthRecordId == Guid.Empty)
        {
            throw new ArgumentException("Health record id is required.", nameof(healthRecordId));
        }

        if (string.IsNullOrWhiteSpace(medicationName))
        {
            throw new ArgumentException("Medication name is required.", nameof(medicationName));
        }

        if (string.IsNullOrWhiteSpace(dosage))
        {
            throw new ArgumentException("Dosage is required.", nameof(dosage));
        }

        if (string.IsNullOrWhiteSpace(frequency))
        {
            throw new ArgumentException("Frequency is required.", nameof(frequency));
        }

        if (durationDays <= 0)
        {
            throw new ArgumentException("Duration must be greater than zero.", nameof(durationDays));
        }

        if (issuedAtUtc == default)
        {
            throw new ArgumentException("Issue time is required.", nameof(issuedAtUtc));
        }

        if (issuedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Issue time must be UTC.", nameof(issuedAtUtc));
        }

        if (expiresAtUtc.HasValue && expiresAtUtc.Value.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Expiry time must be UTC.", nameof(expiresAtUtc));
        }

        if (expiresAtUtc.HasValue && expiresAtUtc.Value <= issuedAtUtc)
        {
            throw new ArgumentException("Expiry time must be after issue time.", nameof(expiresAtUtc));
        }

        var prescription = new Prescription
        {
            Id = Guid.CreateVersion7(),
            DoctorId = doctorId,
            PatientId = patientId,
            HealthRecordId = healthRecordId,
            AppointmentId = appointmentId,
            MedicationName = medicationName.Trim(),
            Dosage = dosage.Trim(),
            Frequency = frequency.Trim(),
            DurationDays = durationDays,
            SpecialInstructions = string.IsNullOrWhiteSpace(specialInstructions) ? null : specialInstructions.Trim(),
            Status = PrescriptionStatus.Active,
            IssuedAtUtc = issuedAtUtc,
            ExpiresAtUtc = expiresAtUtc ?? default
        };

        prescription.ApplyDefaultExpiryIfUnset();

        prescription.RaiseDomainEvent(new PrescriptionIssuedDomainEvent(
            prescription.Id,
            prescription.DoctorId,
            prescription.PatientId,
            prescription.HealthRecordId,
            prescription.IssuedAtUtc,
            prescription.ExpiresAtUtc));

        return prescription;
    }

    public void MarkDispensed(DateTime dispensedAtUtc)
    {
        EnsureEligibleForDispensing(dispensedAtUtc);

        Status = PrescriptionStatus.Dispensed;
        Touch();
    }

    public void EnsureEligibleForDispensing(DateTime asOfUtc)
    {
        if (asOfUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Evaluation time must be UTC.", nameof(asOfUtc));
        }

        if (Status == PrescriptionStatus.Dispensed)
        {
            throw new PrescriptionDispensedException(Id);
        }

        if (Status == PrescriptionStatus.Cancelled || Status == PrescriptionStatus.Expired)
        {
            throw new PrescriptionNotEligibleException(Id, Status);
        }

        if (Status != PrescriptionStatus.Active)
        {
            throw new PrescriptionNotEligibleException(Id, Status);
        }

        if (asOfUtc >= ExpiresAtUtc)
        {
            throw new PrescriptionExpiredException(Id);
        }
    }

    public void ApplyDefaultExpiryIfUnset()
    {
        if (ExpiresAtUtc != default)
        {
            return;
        }

        ExpiresAtUtc = IssuedAtUtc.AddDays(PrescriptionPolicies.DefaultExpiryDays);
    }
}
