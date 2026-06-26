namespace HealthPlatform.Application.PharmacyOrders.Realtime;

public static class PharmacyHubEvents
{
    public const string OrderReceived = "orderReceived";
}

public static class PharmacyGroupNames
{
    public static string ForPharmacy(Guid pharmacyId) => $"pharmacy:{pharmacyId:N}";
}
