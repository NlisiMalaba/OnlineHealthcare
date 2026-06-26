namespace HealthPlatform.API.Requests.Pharmacy;

public sealed class AddInventoryItemRequest
{
    public string MedicationName { get; init; } = string.Empty;

    public string MedicationSku { get; init; } = string.Empty;

    public int Quantity { get; init; }

    public int? LowStockThreshold { get; init; }
}
