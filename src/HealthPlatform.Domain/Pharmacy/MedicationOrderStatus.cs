namespace HealthPlatform.Domain.Pharmacy;

public enum MedicationOrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Preparing = 2,
    Dispatched = 3,
    Delivered = 4,
    Rejected = 5
}
