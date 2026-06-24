using HealthPlatform.Application.Prescriptions;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Prescriptions;

public sealed class LoggingDrugInteractionAlertNotifier(
    ILogger<LoggingDrugInteractionAlertNotifier> logger)
    : IDrugInteractionAlertNotifier
{
    public Task NotifyDrugInteractionAlertAsync(
        Guid doctorUserId,
        Guid patientId,
        string proposedMedicationName,
        string interactingMedicationName,
        string interactionDescription,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Drug interaction alert for doctor user {DoctorUserId}, patient {PatientId}, proposed medication {ProposedMedication}, interacting medication {InteractingMedication}.",
            doctorUserId,
            patientId,
            proposedMedicationName,
            interactingMedicationName);
        return Task.CompletedTask;
    }
}
