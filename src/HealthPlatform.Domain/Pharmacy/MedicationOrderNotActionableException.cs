namespace HealthPlatform.Domain.Pharmacy;

public sealed class MedicationOrderNotActionableException(Guid orderId, MedicationOrderStatus status)
    : Exception($"Medication order '{orderId}' in status '{status}' cannot be updated.")
{
    public MedicationOrderStatus Status { get; } = status;
}
