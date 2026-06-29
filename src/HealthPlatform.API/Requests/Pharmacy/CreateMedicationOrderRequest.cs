using System.Text.Json.Serialization;
using HealthPlatform.Application.PharmacyOrders.CreateMedicationOrder;
using HealthPlatform.Domain.Pharmacy;

namespace HealthPlatform.API.Requests.Pharmacy;

public sealed class CreateMedicationOrderRequest
{
    public Guid PrescriptionId { get; init; }

    public Guid PharmacyId { get; init; }

    public string MedicationSku { get; init; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MedicationDeliveryType DeliveryType { get; init; }

    public string? DeliveryAddress { get; init; }
}
