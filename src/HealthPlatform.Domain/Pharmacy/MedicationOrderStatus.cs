namespace HealthPlatform.Domain.Pharmacy;

public enum MedicationOrderStatus
{
    Pending = 0,
    ClarificationRequested = 1,
    Confirmed = 2,
    Preparing = 3,
    Dispatched = 4,
    Delivered = 5,
    Rejected = 6
}
