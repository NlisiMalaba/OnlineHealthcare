namespace HealthPlatform.API.Requests.Prescriptions;

public sealed class CreatePrescriptionRequest
{
    public Guid PatientId { get; init; }

    public string MedicationName { get; init; } = string.Empty;

    public string Dosage { get; init; } = string.Empty;

    public string Frequency { get; init; } = string.Empty;

    public int DurationDays { get; init; }

    public string? SpecialInstructions { get; init; }

    public DateTime? ExpiresAtUtc { get; init; }

    public Guid? AppointmentId { get; init; }
}
