namespace HealthPlatform.Application.Prescriptions;

public sealed record PrescriptionDto(
    Guid Id,
    Guid DoctorId,
    Guid PatientId,
    Guid HealthRecordId,
    Guid? AppointmentId,
    string MedicationName,
    string Dosage,
    string Frequency,
    int DurationDays,
    string? SpecialInstructions,
    string Status,
    DateTime IssuedAtUtc,
    DateTime ExpiresAtUtc);
