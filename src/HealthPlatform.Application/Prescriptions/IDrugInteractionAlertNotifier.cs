namespace HealthPlatform.Application.Prescriptions;

public interface IDrugInteractionAlertNotifier
{
    Task NotifyDrugInteractionAlertAsync(
        Guid doctorUserId,
        Guid patientId,
        string proposedMedicationName,
        string interactingMedicationName,
        string interactionDescription,
        CancellationToken ct);
}
