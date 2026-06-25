using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Prescriptions.CreatePrescription;

public sealed record CreatePrescriptionCommand(
    Guid PatientId,
    string MedicationName,
    string Dosage,
    string Frequency,
    int DurationDays,
    string? SpecialInstructions,
    DateTime? ExpiresAtUtc,
    Guid? AppointmentId) : ICommand<PrescriptionDto>;
