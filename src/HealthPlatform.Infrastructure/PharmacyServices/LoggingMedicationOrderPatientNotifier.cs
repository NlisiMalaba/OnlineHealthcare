using HealthPlatform.Application.PharmacyOrders;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.PharmacyServices;

public sealed class LoggingMedicationOrderPatientNotifier(
    ILogger<LoggingMedicationOrderPatientNotifier> logger)
    : IMedicationOrderPatientNotifier
{
    public Task NotifyOrderStatusChangedAsync(
        Guid patientUserId,
        Guid orderId,
        string previousStatus,
        string newStatus,
        string? trackingUrl,
        IReadOnlyList<PharmacyOrderAlternativeDto>? alternativePharmacies,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        logger.LogInformation(
            "Medication order {OrderId} status changed from {PreviousStatus} to {NewStatus} for patient user {PatientUserId}. TrackingUrl present: {HasTrackingUrl}. Alternative pharmacies: {AlternativeCount}.",
            orderId,
            previousStatus,
            newStatus,
            patientUserId,
            !string.IsNullOrWhiteSpace(trackingUrl),
            alternativePharmacies?.Count ?? 0);

        return Task.CompletedTask;
    }
}
