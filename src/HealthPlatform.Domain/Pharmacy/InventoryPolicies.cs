namespace HealthPlatform.Domain.Pharmacy;

public static class InventoryPolicies
{
    public const int DefaultLowStockThreshold = 10;

    public static bool ShouldRaiseLowStockAlert(int previousQuantity, int newQuantity, int lowStockThreshold) =>
        previousQuantity > lowStockThreshold && newQuantity <= lowStockThreshold;
}
