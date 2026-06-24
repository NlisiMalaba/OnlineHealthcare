using HealthPlatform.Domain.Prescriptions;

namespace HealthPlatform.Application.Prescriptions;

public static class PrescriptionMappings
{
    public static PrescriptionDto ToDto(this Prescription prescription) =>
        new(
            prescription.Id,
            prescription.DoctorId,
            prescription.PatientId,
            prescription.HealthRecordId,
            prescription.AppointmentId,
            prescription.MedicationName,
            prescription.Dosage,
            prescription.Frequency,
            prescription.DurationDays,
            prescription.SpecialInstructions,
            prescription.Status.ToString().ToLowerInvariant(),
            prescription.IssuedAtUtc,
            prescription.ExpiresAtUtc,
            prescription.CancellationReason);
}
