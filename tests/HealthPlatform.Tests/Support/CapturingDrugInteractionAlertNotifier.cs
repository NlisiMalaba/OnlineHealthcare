using HealthPlatform.Application.Prescriptions;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingDrugInteractionAlertNotifier : IDrugInteractionAlertNotifier
{
    public List<DrugInteractionAlertCall> Calls { get; } = [];

    public Task NotifyDrugInteractionAlertAsync(
        Guid doctorUserId,
        Guid patientId,
        string proposedMedicationName,
        string interactingMedicationName,
        string interactionDescription,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Calls.Add(new DrugInteractionAlertCall(
            doctorUserId,
            patientId,
            proposedMedicationName,
            interactingMedicationName,
            interactionDescription));
        return Task.CompletedTask;
    }

    public sealed record DrugInteractionAlertCall(
        Guid DoctorUserId,
        Guid PatientId,
        string ProposedMedicationName,
        string InteractingMedicationName,
        string InteractionDescription);
}
