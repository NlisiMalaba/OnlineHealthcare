namespace HealthPlatform.Application.Prescriptions;

public interface IPrescriptionCancelledNotifier
{
    Task NotifyPrescriptionCancelledAsync(
        Guid patientUserId,
        Guid prescriptionId,
        CancellationToken ct);
}
