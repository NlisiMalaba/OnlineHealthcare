using HealthPlatform.Application.PharmacyOrders.CompleteMedicationOrderFulfillment;
using HealthPlatform.Application.PharmacyOrders.ConfirmMedicationOrder;
using HealthPlatform.Application.PharmacyOrders.Inventory.AddInventoryItem;
using HealthPlatform.Application.PharmacyOrders.Inventory.UpdateInventoryItemQuantity;
using HealthPlatform.Application.PharmacyOrders.RejectMedicationOrder;
using HealthPlatform.Application.Search;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.PharmacyOrders;

public sealed class PharmacyOrderRejectionPickupAndStockSyncTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Reject_order_searches_alternatives_by_medication_sku_with_stock_filter()
    {
        var context = await MedicationOrderTestData.SeedPendingDeliveryOrderAsync(_host, "MED-REJECT-001");
        _host.SearchService.SeedPharmacySearchResults(
        [
            new(context.Pharmacy.Id, "Rejecting Pharmacy", "1 Main St", true, null),
            new(Guid.CreateVersion7(), "Alt Pharmacy", "2 Second St", true, null)
        ]);

        _host.CurrentUser.UserId = context.Pharmacy.UserId;

        await _host.Sender.Send(
            new RejectMedicationOrderCommand(context.Order.Id, "Cannot fulfill today"),
            CancellationToken.None);

        var criteria = _host.SearchService.LastPharmacySearchCriteria;
        Assert.NotNull(criteria);
        Assert.Equal("MED-REJECT-001", criteria.MedicationSku);
        Assert.True(criteria.HasStock);
    }

    [Fact]
    public async Task Reject_order_excludes_out_of_stock_pharmacies_from_alternatives()
    {
        var context = await MedicationOrderTestData.SeedPendingDeliveryOrderAsync(_host);
        var inStockPharmacyId = Guid.CreateVersion7();
        var outOfStockPharmacyId = Guid.CreateVersion7();

        _host.SearchService.SeedPharmacySearchResults(
        [
            new(context.Pharmacy.Id, "Rejecting Pharmacy", "1 Main St", true, null),
            new(inStockPharmacyId, "In Stock Pharmacy", "2 Second St", true, null),
            new(outOfStockPharmacyId, "Out Of Stock Pharmacy", "3 Third St", false, null)
        ]);

        _host.CurrentUser.UserId = context.Pharmacy.UserId;

        await _host.Sender.Send(
            new RejectMedicationOrderCommand(context.Order.Id, "Out of stock after verification"),
            CancellationToken.None);

        var notification = Assert.Single(_host.MedicationOrderPatientNotifier.Notifications);
        Assert.NotNull(notification.Alternatives);
        Assert.Contains(notification.Alternatives, alt => alt.PharmacyId == inStockPharmacyId);
        Assert.DoesNotContain(notification.Alternatives, alt => alt.PharmacyId == outOfStockPharmacyId);
        Assert.DoesNotContain(notification.Alternatives, alt => alt.PharmacyId == context.Pharmacy.Id);
    }

    [Fact]
    public async Task Reject_order_suggests_at_most_five_alternative_pharmacies()
    {
        var context = await MedicationOrderTestData.SeedPendingDeliveryOrderAsync(_host);
        var alternatives = Enumerable.Range(0, 7)
            .Select(index => new PharmacySearchMatchDto(
                Guid.CreateVersion7(),
                $"Alt Pharmacy {index}",
                $"{index} Alt Street",
                true,
                null))
            .ToList();

        _host.SearchService.SeedPharmacySearchResults(
        [
            new(context.Pharmacy.Id, "Rejecting Pharmacy", "1 Main St", true, null),
            .. alternatives
        ]);

        _host.CurrentUser.UserId = context.Pharmacy.UserId;

        await _host.Sender.Send(
            new RejectMedicationOrderCommand(context.Order.Id, "Capacity reached"),
            CancellationToken.None);

        var notification = Assert.Single(_host.MedicationOrderPatientNotifier.Notifications);
        Assert.NotNull(notification.Alternatives);
        Assert.InRange(notification.Alternatives.Count, 1, 5);
        Assert.DoesNotContain(notification.Alternatives, alt => alt.PharmacyId == context.Pharmacy.Id);
    }

    [Fact]
    public async Task Pickup_flow_notifies_patient_on_confirm_and_pickup_complete()
    {
        var context = await MedicationOrderTestData.SeedPendingPickupOrderAsync(_host);
        _host.CurrentUser.UserId = context.Pharmacy.UserId;

        await _host.Sender.Send(new ConfirmMedicationOrderCommand(context.Order.Id), CancellationToken.None);
        await _host.Sender.Send(
            new CompleteMedicationOrderFulfillmentCommand(context.Order.Id),
            CancellationToken.None);

        Assert.Equal(2, _host.MedicationOrderPatientNotifier.Notifications.Count);
        Assert.Equal(context.Patient.UserId, _host.MedicationOrderPatientNotifier.Notifications[0].PatientUserId);
        Assert.Equal("confirmed", _host.MedicationOrderPatientNotifier.Notifications[0].NewStatus);
        Assert.Null(_host.MedicationOrderPatientNotifier.Notifications[0].TrackingUrl);
        Assert.Equal("delivered", _host.MedicationOrderPatientNotifier.Notifications[1].NewStatus);
    }

    [Fact]
    public async Task Inventory_quantity_update_syncs_complete_pharmacy_stock_summary_to_search_index()
    {
        var pharmacy = await SeedVerifiedPharmacyAsync();
        _host.CurrentUser.UserId = pharmacy.UserId;

        await _host.Sender.Send(
            new AddInventoryItemCommand("Paracetamol", "MED-SYNC-A", 40, null),
            CancellationToken.None);

        var secondItem = await _host.Sender.Send(
            new AddInventoryItemCommand("Ibuprofen", "MED-SYNC-B", 20, null),
            CancellationToken.None);

        await _host.Sender.Send(
            new UpdateInventoryItemQuantityCommand(secondItem.Id, 12),
            CancellationToken.None);

        var latestSync = _host.SearchService.PharmacyStockUpdates[^1];
        Assert.Equal(pharmacy.Id, latestSync.PharmacyId);
        Assert.Equal(2, latestSync.Stock.Count);

        var stockBySku = latestSync.Stock.ToDictionary(entry => entry.MedicationSku);
        Assert.Equal(40, stockBySku["MED-SYNC-A"].QuantityOnHand);
        Assert.Equal(12, stockBySku["MED-SYNC-B"].QuantityOnHand);
    }

    private async Task<Pharmacy> SeedVerifiedPharmacyAsync()
    {
        var registration = await _host.Sender.Send(
            PharmacyRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        var pharmacy = await _host.DbContext.Pharmacies.SingleAsync(p => p.Id == registration.PharmacyId);
        pharmacy.MarkVerified();
        await _host.DbContext.SaveChangesAsync();
        return pharmacy;
    }
}
