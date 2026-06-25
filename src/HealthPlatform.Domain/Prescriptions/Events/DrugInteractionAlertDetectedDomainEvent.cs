using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Prescriptions.Events;

public sealed record DrugInteractionAlertDetectedDomainEvent(
    Guid DoctorId,
    Guid PatientId,
    string ProposedMedicationName,
    string InteractingMedicationName,
    string InteractionDescription) : DomainEvent;
