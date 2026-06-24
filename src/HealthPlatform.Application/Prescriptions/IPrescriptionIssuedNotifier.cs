namespace HealthPlatform.Application.Prescriptions;

public interface IPrescriptionIssuedNotifier
{
    Task NotifyPrescriptionIssuedAsync(
        Guid patientUserId,
        Guid prescriptionId,
        DateTime issuedAtUtc,
        CancellationToken ct);
}
