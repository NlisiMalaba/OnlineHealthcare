using MediatR;

namespace HealthPlatform.Application.Prescriptions.Notifications;

public sealed record DrugInteractionAlertDetectedNotification(
    Guid DoctorId,
    Guid PatientId,
    string ProposedMedicationName,
    string InteractingMedicationName,
    string InteractionDescription,
    DateTime OccurredAtUtc) : INotification;
