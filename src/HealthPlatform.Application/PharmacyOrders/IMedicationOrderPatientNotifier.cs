namespace HealthPlatform.Application.PharmacyOrders;

public sealed record PharmacyOrderAlternativeDto(Guid PharmacyId, string Name, string Address);

public interface IMedicationOrderPatientNotifier
{
    Task NotifyOrderStatusChangedAsync(
        Guid patientUserId,
        Guid orderId,
        string previousStatus,
        string newStatus,
        string? trackingUrl,
        IReadOnlyList<PharmacyOrderAlternativeDto>? alternativePharmacies,
        CancellationToken ct);
}
