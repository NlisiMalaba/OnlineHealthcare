using HealthPlatform.Application.Prescriptions;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Prescriptions;

public sealed class LoggingPrescriptionCancelledNotifier(
    ILogger<LoggingPrescriptionCancelledNotifier> logger)
    : IPrescriptionCancelledNotifier
{
    public Task NotifyPrescriptionCancelledAsync(
        Guid patientUserId,
        Guid prescriptionId,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Prescription cancelled notification requested for prescription {PrescriptionId}, patient user {PatientUserId}.",
            prescriptionId,
            patientUserId);
        return Task.CompletedTask;
    }
}
