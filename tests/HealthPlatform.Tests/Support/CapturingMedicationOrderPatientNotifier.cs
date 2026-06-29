using HealthPlatform.Application.PharmacyOrders;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingMedicationOrderPatientNotifier : IMedicationOrderPatientNotifier
{
    public List<(
        Guid PatientUserId,
        Guid OrderId,
        string PreviousStatus,
        string NewStatus,
        string? TrackingUrl,
        IReadOnlyList<PharmacyOrderAlternativeDto>? Alternatives)> Notifications { get; } = [];

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
        Notifications.Add((
            patientUserId,
            orderId,
            previousStatus,
            newStatus,
            trackingUrl,
            alternativePharmacies));
        return Task.CompletedTask;
    }
}
