namespace HealthPlatform.API.Requests.Prescriptions;

public sealed class CancelPrescriptionRequest
{
    public string Reason { get; init; } = string.Empty;
}
