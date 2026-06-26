using HealthPlatform.Application.Identity.RegisterPharmacy;
using HealthPlatform.Application.PharmacyOrders.Inventory.AddInventoryItem;
using HealthPlatform.Application.PharmacyOrders.Inventory.MarkInventoryItemOutOfStock;
using HealthPlatform.Application.PharmacyOrders.Inventory.UpdateInventoryItemQuantity;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Pharmacy;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.PharmacyOrders;

public sealed class InventoryManagementCommandHandlerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task AddInventoryItem_publishes_stock_summary_to_search_index()
    {
        var pharmacy = await SeedVerifiedPharmacyAsync();
        _host.CurrentUser.UserId = pharmacy.UserId;

        var item = await _host.Sender.Send(
            new AddInventoryItemCommand("Paracetamol", "MED-001", 50, null),
            CancellationToken.None);

        Assert.Equal(50, item.Quantity);
        Assert.False(item.IsOutOfStock);
        Assert.Single(_host.SearchService.PharmacyStockUpdates);
        Assert.Equal(pharmacy.Id, _host.SearchService.PharmacyStockUpdates[0].PharmacyId);
        Assert.Equal("MED-001", _host.SearchService.PharmacyStockUpdates[0].Stock[0].MedicationSku);
        Assert.Equal(50, _host.SearchService.PharmacyStockUpdates[0].Stock[0].QuantityOnHand);
    }

    [Fact]
    public async Task UpdateInventoryItemQuantity_syncs_updated_stock_to_search_index()
    {
        var pharmacy = await SeedVerifiedPharmacyAsync();
        _host.CurrentUser.UserId = pharmacy.UserId;

        var item = await _host.Sender.Send(
            new AddInventoryItemCommand("Ibuprofen", "MED-002", 20, null),
            CancellationToken.None);

        var updated = await _host.Sender.Send(
            new UpdateInventoryItemQuantityCommand(item.Id, 75),
            CancellationToken.None);

        Assert.Equal(75, updated.Quantity);
        Assert.Equal(2, _host.SearchService.PharmacyStockUpdates.Count);
        Assert.Equal(75, _host.SearchService.PharmacyStockUpdates[^1].Stock.Single(s => s.MedicationSku == "MED-002").QuantityOnHand);
    }

    [Fact]
    public async Task MarkInventoryItemOutOfStock_sets_quantity_zero_and_syncs_search_index()
    {
        var pharmacy = await SeedVerifiedPharmacyAsync();
        _host.CurrentUser.UserId = pharmacy.UserId;

        var item = await _host.Sender.Send(
            new AddInventoryItemCommand("Amoxicillin", "MED-003", 15, null),
            CancellationToken.None);

        var marked = await _host.Sender.Send(
            new MarkInventoryItemOutOfStockCommand(item.Id),
            CancellationToken.None);

        Assert.Equal(0, marked.Quantity);
        Assert.True(marked.IsOutOfStock);
        Assert.Equal(0, _host.SearchService.PharmacyStockUpdates[^1].Stock.Single(s => s.MedicationSku == "MED-003").QuantityOnHand);
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
