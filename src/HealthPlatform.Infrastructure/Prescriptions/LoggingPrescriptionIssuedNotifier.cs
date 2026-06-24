using HealthPlatform.Application.Prescriptions;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Prescriptions;

public sealed class LoggingPrescriptionIssuedNotifier(
    ILogger<LoggingPrescriptionIssuedNotifier> logger)
    : IPrescriptionIssuedNotifier
{
    public Task NotifyPrescriptionIssuedAsync(
        Guid patientUserId,
        Guid prescriptionId,
        DateTime issuedAtUtc,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Prescription issued notification requested for prescription {PrescriptionId}, patient user {PatientUserId}.",
            prescriptionId,
            patientUserId);
        return Task.CompletedTask;
    }
}
