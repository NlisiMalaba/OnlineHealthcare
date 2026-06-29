using FsCheck.Xunit;
using HealthPlatform.Application.PharmacyOrders.Inventory.AddInventoryItem;
using HealthPlatform.Application.PharmacyOrders.Inventory.MarkInventoryItemOutOfStock;
using HealthPlatform.Application.PharmacyOrders.Inventory.UpdateInventoryItemQuantity;
using HealthPlatform.Domain.Pharmacy;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Tests.Properties;

public sealed class LowStockAlertThresholdPropertyTests
{
    // Feature: online-healthcare-platform, Property 25: Low Stock Alert Threshold
    [Property(Arbitrary = [typeof(LowStockAlertArbitraries)], MaxTest = 100)]
    public bool Low_stock_alert_is_emitted_iff_quantity_crosses_below_pharmacy_threshold(
        LowStockAlertUpdateCase updateCase) =>
        RunThresholdCrossingInvariantAsync(updateCase).GetAwaiter().GetResult();

    private static async Task<bool> RunThresholdCrossingInvariantAsync(LowStockAlertUpdateCase updateCase)
    {
        var shouldAlert = InventoryPolicies.ShouldRaiseLowStockAlert(
            updateCase.PreviousQuantity,
            updateCase.NewQuantity,
            updateCase.LowStockThreshold);

        await using var host = new PatientRegistrationTestHost();

        var registration = await host.Sender.Send(
            PharmacyRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        var pharmacy = await host.DbContext.Pharmacies.SingleAsync(p => p.Id == registration.PharmacyId);
        pharmacy.MarkVerified();
        await host.DbContext.SaveChangesAsync();

        host.CurrentUser.UserId = pharmacy.UserId;

        var medicationSku = $"SKU-{Guid.NewGuid():N}"[..16];
        var item = await host.Sender.Send(
            new AddInventoryItemCommand(
                "Property Test Medication",
                medicationSku,
                updateCase.PreviousQuantity,
                updateCase.LowStockThreshold),
            CancellationToken.None);

        host.LowStockAlertNotifier.Notifications.Clear();

        if (updateCase.UseMarkOutOfStock)
        {
            await host.Sender.Send(
                new MarkInventoryItemOutOfStockCommand(item.Id),
                CancellationToken.None);
        }
        else
        {
            await host.Sender.Send(
                new UpdateInventoryItemQuantityCommand(item.Id, updateCase.NewQuantity),
                CancellationToken.None);
        }

        if (shouldAlert)
        {
            if (host.LowStockAlertNotifier.Notifications.Count != 1)
            {
                return false;
            }

            var alert = host.LowStockAlertNotifier.Notifications[0];
            return alert.PharmacyUserId == pharmacy.UserId
                && alert.InventoryItemId == item.Id
                && alert.MedicationSku == medicationSku
                && alert.Quantity == updateCase.NewQuantity
                && alert.LowStockThreshold == updateCase.LowStockThreshold;
        }

        return host.LowStockAlertNotifier.Notifications.Count == 0;
    }
}
