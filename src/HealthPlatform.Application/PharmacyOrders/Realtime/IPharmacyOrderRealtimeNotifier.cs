namespace HealthPlatform.Application.PharmacyOrders.Realtime;

public sealed record PharmacyOrderReceivedRealtimeDto(
    Guid OrderId,
    Guid PatientId,
    Guid PrescriptionId,
    string MedicationSku,
    string MedicationName,
    string Dosage,
    string Frequency,
    int DurationDays,
    string? SpecialInstructions,
    string DeliveryType,
    string? DeliveryAddress,
    DateTime PlacedAtUtc);

public interface IPharmacyOrderRealtimeNotifier
{
    Task PublishOrderReceivedAsync(
        Guid pharmacyId,
        PharmacyOrderReceivedRealtimeDto order,
        CancellationToken ct);
}
