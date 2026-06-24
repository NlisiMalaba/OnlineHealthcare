using HealthPlatform.API.Requests.Prescriptions;
using HealthPlatform.Application.Prescriptions.CreatePrescription;

namespace HealthPlatform.API.Mapping;

public static class PrescriptionCommandMapper
{
    public static CreatePrescriptionCommand ToCreateCommand(CreatePrescriptionRequest request) =>
        new(
            request.PatientId,
            request.MedicationName,
            request.Dosage,
            request.Frequency,
            request.DurationDays,
            request.SpecialInstructions,
            request.ExpiresAtUtc,
            request.AppointmentId);
}
