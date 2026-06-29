namespace HealthPlatform.API.Requests.Pharmacy;

public sealed class RejectMedicationOrderRequest
{
    public string Reason { get; init; } = string.Empty;
}
